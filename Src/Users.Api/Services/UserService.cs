using System.ComponentModel.DataAnnotations; // <-- AGREGADO PARA VALIDAR FORMATO
using Users.Api.Data;
using Users.Api.DTOs;
using Users.Api.Exceptions;

namespace Users.Api.Services;

public class UserService
{
    private readonly UserRepository _repo;
    public UserService(UserRepository repo) => _repo = repo;

    public async Task<UserLoginResponse> LogInAsync(LogInUserRequest request)
    {
        
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.PasswordHash))
            throw new BusinessRuleException("USR-002", "Los datos del usuario son inválidos. El email y la contraseña son requeridos.");

        
        var emailChecker = new EmailAddressAttribute();
        if (!emailChecker.IsValid(request.Email))
            throw new BusinessRuleException("USR-002", "Los datos del usuario son inválidos. El formato del correo electrónico no es válido.");

        
        var user = await _repo.GetByEmailAsync(request.Email);

        
        if (user is null)
            throw new BusinessRuleException("USR-003", "Credenciales incorrectas.");

       
        if (!user.Activo || user.IntentosFallidos >= 3)
            throw new BusinessRuleException("USR-004", "Usuario bloqueado por demasiados intentos fallidos.");

       
        if (user.PasswordHash != request.PasswordHash)
        {
            user.IntentosFallidos += 1;

            if (user.IntentosFallidos >= 3)
            {
                await _repo.UpdateBlockStatusAsync(user.Id, user.IntentosFallidos, activo: false);
                throw new BusinessRuleException("USR-004", "Usuario bloqueado por demasiados intentos fallidos.");
            }

            await _repo.UpdateBlockStatusAsync(user.Id, user.IntentosFallidos, activo: true);
            throw new BusinessRuleException("USR-003", "Credenciales incorrectas.");
        }

        // Éxito: Resetea contador a 0
        if (user.IntentosFallidos > 0)
            await _repo.UpdateBlockStatusAsync(user.Id, 0, activo: true);

        return new UserLoginResponse
        {
            Id = user.Id,
            Nombre = user.Nombre,
            Apellido = user.Apellido,
            Email = user.Email
        };
    }
}