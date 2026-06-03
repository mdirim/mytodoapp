using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyTodoApp.Models;

namespace MyTodoApp.Controllers;

public class TodosController : Controller
{
    private readonly AppDbContext _db;

    public TodosController(AppDbContext db)
    {
        _db = db;
    }

    // GET: /Todos
    public async Task<IActionResult> Index(string? filter)
    {
        var items = _db.TodoItems.AsQueryable();

        filter = filter?.ToLower() switch
        {
            "done" => "done",
            "pending" => "pending",
            _ => null
        };

        items = filter switch
        {
            "done" => items.Where(t => t.IsDone),
            "pending" => items.Where(t => !t.IsDone),
            _ => items
        };

        var list = await items
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        ViewBag.CurrentFilter = filter;
        ViewBag.TotalCount = await _db.TodoItems.CountAsync();
        ViewBag.DoneCount = await _db.TodoItems.CountAsync(t => t.IsDone);
        ViewBag.PendingCount = await _db.TodoItems.CountAsync(t => !t.IsDone);

        return View(list);
    }

    // GET: /Todos/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var item = await _db.TodoItems.FindAsync(id);
        if (item is null) return NotFound();
        return View(item);
    }

    // GET: /Todos/Create
    public IActionResult Create() => View();

    // POST: /Todos/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,Description,DueDate")] TodoItem item)
    {
        if (ModelState.IsValid)
        {
            item.CreatedAt = DateTime.Now;
            item.IsDone = false;
            _db.Add(item);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Görev başarıyla eklendi! ✅";
            return RedirectToAction(nameof(Index));
        }
        return View(item);
    }

    // GET: /Todos/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var item = await _db.TodoItems.FindAsync(id);
        if (item is null) return NotFound();
        return View(item);
    }

    // POST: /Todos/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,IsDone,CreatedAt,DueDate")] TodoItem item)
    {
        if (id != item.Id) return NotFound();
        if (ModelState.IsValid)
        {
            try
            {
                _db.Update(item);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Görev güncellendi! ✏️";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TodoItemExists(item.Id)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(item);
    }

    // GET: /Todos/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var item = await _db.TodoItems.FindAsync(id);
        if (item is null) return NotFound();
        return View(item);
    }

    // POST: /Todos/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _db.TodoItems.FindAsync(id);
        if (item is not null)
        {
            _db.TodoItems.Remove(item);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Görev silindi! 🗑️";
        }
        return RedirectToAction(nameof(Index));
    }

    // POST: /Todos/Toggle/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        var item = await _db.TodoItems.FindAsync(id);
        if (item is null) return NotFound();
        item.IsDone = !item.IsDone;
        await _db.SaveChangesAsync();
        TempData["Success"] = item.IsDone
            ? "Tebrikler! Görev tamamlandı! 🎉"
            : "Görev tekrar aktif edildi!";
        return RedirectToAction(nameof(Index));
    }

    private bool TodoItemExists(int id) =>
        _db.TodoItems.Any(e => e.Id == id);
}
