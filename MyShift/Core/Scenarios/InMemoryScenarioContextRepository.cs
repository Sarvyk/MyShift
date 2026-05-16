using MyShift.Core.Scenarios.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShift.Core.Scenarios
{
    internal class InMemoryScenarioContextRepository : IScenarioContextRepository
    {
        private readonly ConcurrentDictionary<long, ScenarioContext> _context = new();//тут будут храниться сценарии пользователей. Типа диалоги с сохранением состояния
        public async Task<ScenarioContext?> GetContext(long userId, CancellationToken ct)
        {
            if (_context.ContainsKey(userId))
                return _context[userId];
            return null;
        }

        public async Task<IReadOnlyList<KeyValuePair<long, ScenarioContext>>> GetContexts(CancellationToken ct)
        {
            var contexts = _context.ToList();
            return contexts;
        }

        public async Task ResetContext(long userId, CancellationToken ct)
        {
            if (_context.ContainsKey(userId))
                _context.TryRemove(userId, out ScenarioContext value);
        }

        public async Task SetContext(long userId, ScenarioContext context, CancellationToken ct)
        {
            if (_context.ContainsKey(userId))
                _context[userId] = context;
            else
                _context.TryAdd(userId, context);
        }
    }
}
