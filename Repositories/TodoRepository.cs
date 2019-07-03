using Pegasus_backend.pegasusContext;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pegasus_backend.Utilities;
using Pegasus_backend.Models;

namespace Pegasus_backend.Repositories
{
    public class TodoRepository
    {
        private readonly ablemusicContext _ablemusicContext;

        public TodoRepository()
        {
            _ablemusicContext = new ablemusicContext();
            _todoLists = new List<TodoList>();
        }

        public List<TodoList> _todoLists { get; }

        public void AddSingleTodoList(string listName, string listContent, short userId, DateTime todoDate, int? lessonId, int? learnerId, short? teacherId)
        {
            _todoLists.Add(new TodoList
            {
                ListName = listName,
                ListContent = listContent,
                CreatedAt = DateTime.UtcNow.ToNZTimezone(),
                ProcessedAt = null,
                ProcessFlag = 0,
                UserId = userId,
                TodoDate = todoDate,
                LessonId = lessonId,
                LearnerId = learnerId,
                TeacherId = teacherId
            });
        }

        public void AddMutipleTodoLists(string listName, Dictionary<int,string>learnerIdContent, short userId, DateTime todoDate, int? lessonId, short? teacherId)
        {
            foreach(KeyValuePair<int, string> lc in learnerIdContent)
            {
                AddSingleTodoList(listName, lc.Value, userId, todoDate, lessonId, lc.Key, teacherId);
            }
        }

        public void AddMutipleTodoLists(string listName, Dictionary<short, string> teacherIdContent, short userId, DateTime todoDate, int? lessonId, int? learnerId)
        {
            foreach(KeyValuePair<short, string> tc in teacherIdContent)
            {
                AddSingleTodoList(listName, tc.Value, userId, todoDate, lessonId, learnerId, tc.Key);
            }
        }

        public async Task<Result<List<TodoList>>> SaveTodoListsAsync()
        {
            var result = new Result<List<TodoList>>();
            try
            {
                foreach(var t in _todoLists)
                {
                    await _ablemusicContext.TodoList.AddAsync(t);
                }
                await _ablemusicContext.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
            result.Data = _todoLists;
            return result;
        }

    }
}
