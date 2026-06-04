using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MyTodoApp.Models;

public class TodoItem
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Başlık zorunludur")]
    [StringLength(200, MinimumLength = 1)]
    [DisplayName("Başlık")]
    public string Title { get; set; } = string.Empty;

    [DisplayName("Açıklama")]
    public string? Description { get; set; }

    [DisplayName("Tamamlandı mı?")]
    public bool IsDone { get; set; }

    [DisplayName("Oluşturulma Tarihi")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [DisplayName("Son Tarih")]
    [DataType(DataType.Date)]
    public DateTime? DueDate { get; set; }
}
