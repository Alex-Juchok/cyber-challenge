using System;
using System.ComponentModel.DataAnnotations;
using CyberChallenge.Data.Common.Models;

public class GameLevel : BaseDeletableModel<Guid>
{
    public GameLevel()
    {
        this.Id = Guid.NewGuid();
    }

    /// <summary>
    /// Номер уровня.
    /// </summary>
    public int LevelNumber { get; set; }

    /// <summary>
    /// Заголовок уровня.
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Описание уровня.
    /// </summary>
    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
}