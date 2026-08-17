using FrontlineCardWarfare.Data;
using Microsoft.EntityFrameworkCore;

namespace FrontlineCardWarfare.Repositories;

/// <summary>
/// Репозиторий для работы с пользователями.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly GameDbContext _context;

    /// <summary>
    /// Инициализирует новый экземпляр UserRepository.
    /// </summary>
    /// <param name="context">Контекст базы данных.</param>
    public UserRepository(GameDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Получает пользователя по имени.
    /// </summary>
    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    /// <summary>
    /// Получает пользователя по ID.
    /// </summary>
    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    /// <summary>
    /// Добавляет нового пользователя.
    /// </summary>
    public async Task AddUserAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Обновляет существующего пользователя.
    /// </summary>
    public async Task UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Получает всех пользователей.
    /// </summary>
    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.Users
            .Include(u => u.Statistics)
            .ToListAsync();
    }

    /// <summary>
    /// Получает всех заблокированных пользователей.
    /// </summary>
    public async Task<List<User>> GetBlockedUsersAsync()
    {
        return await _context.Users
            .Where(u => u.IsBlocked)
            .ToListAsync();
    }
}
