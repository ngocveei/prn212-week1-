using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TaskSchedulerDemo
{
    // Simple task priority enum
    public enum TaskPriority
    {
        Low,
        Normal,
        High
    }

    // Interface for scheduled tasks
    public interface IScheduledTask
    {
        string Name { get; }
        TaskPriority Priority { get; }
        TimeSpan Interval { get; }
        DateTime LastRun { get; }
        Task ExecuteAsync();
    }

    // Basic implementation of a scheduled task
    public class SimpleTask : IScheduledTask
    {
        private readonly Func<Task> _action;
        private DateTime _lastRun = DateTime.MinValue;

        public string Name { get; }
        public TaskPriority Priority { get; }
        public TimeSpan Interval { get; }
        public DateTime LastRun => _lastRun;

        public SimpleTask(string name, TaskPriority priority, TimeSpan interval, Func<Task> action)
        {
            Name = name;
            Priority = priority;
            Interval = interval;
            _action = action;
        }

        public async Task ExecuteAsync()
        {
            await _action();
            _lastRun = DateTime.Now;
        }
    }

    // Implementation of the scheduler
    public class TaskScheduler
    {
        // Underlying list to store tasks
        private readonly List<IScheduledTask> _tasks = new List<IScheduledTask>();
        private readonly object _lock = new object();

        public TaskScheduler()
        {
            // Nothing to initialize beyond the list
        }

        // Thêm một nhiệm vụ vào danh sách
        public void AddTask(IScheduledTask task)
        {
            lock (_lock)
            {
                // Tránh thêm trùng tên
                if (_tasks.Any(t => t.Name == task.Name))
                    throw new ArgumentException($"Task with name '{task.Name}' already exists.");

                _tasks.Add(task);
            }
        }

        
        public void RemoveTask(string taskName)
        {
            lock (_lock)
            {
                var existing = _tasks.FirstOrDefault(t => t.Name == taskName);
                if (existing != null)
                    _tasks.Remove(existing);
            }
        }


        public List<IScheduledTask> GetScheduledTasks()
        {
            lock (_lock)
            {
                // Trả về bản sao để tránh chỉnh sửa trực tiếp
                return new List<IScheduledTask>(_tasks);
            }
        }

     
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                List<IScheduledTask> dueTasks;
                DateTime now = DateTime.Now;

                lock (_lock)
                {
                    // Lọc các nhiệm vụ đã đến hạn
                    dueTasks = _tasks
                        .Where(t => now - t.LastRun >= t.Interval)
                        // Ưu tiên cao trước
                        .OrderByDescending(t => t.Priority)
                        .ToList();
                }

                foreach (var task in dueTasks)
                {
                    try
                    {
                        await task.ExecuteAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error executing task '{task.Name}': {ex.Message}");
                    }

                    // Nếu token đã hủy, thoát ngay
                    if (cancellationToken.IsCancellationRequested)
                        break;
                }

                try
                {
                    await Task.Delay(500, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                   
                    break;
                }
            }
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Task Scheduler Demo");
            var scheduler = new TaskScheduler();

            // Thêm các nhiệm vụ mẫu
            scheduler.AddTask(new SimpleTask(
                "High Priority Task",
                TaskPriority.High,
                TimeSpan.FromSeconds(2),
                async () =>
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Running high priority task");
                    await Task.Delay(500);
                }
            ));

            scheduler.AddTask(new SimpleTask(
                "Normal Priority Task",
                TaskPriority.Normal,
                TimeSpan.FromSeconds(3),
                async () =>
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Running normal priority task");
                    await Task.Delay(300);
                }
            ));

            scheduler.AddTask(new SimpleTask(
                "Low Priority Task",
                TaskPriority.Low,
                TimeSpan.FromSeconds(4),
                async () =>
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Running low priority task");
                    await Task.Delay(200);
                }
            ));

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            Console.WriteLine("Press any key to stop the scheduler...");

         
            var schedulerTask = scheduler.StartAsync(cts.Token);

           
            Console.ReadKey();
            cts.Cancel();

            try
            {
                await schedulerTask;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Scheduler stopped by cancellation.");
            }

            Console.WriteLine("Scheduler demo finished!");
        }
    }
}
