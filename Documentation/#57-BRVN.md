### Название задачи
Нумерация ББЛ в порядке "Обращение обратного порядка обхода".

#### Постановка задачи
Реализовать возможность нумерации ББл в порядке "Обращение обратного порядка обхода".

#### Зависимости задач в графе задач
Зависит от:
* Control Flow Graph
  
От задачи зависит: 
* Модифицированный итерационный алгоритм с нумерацией базовых блоков и подсчётом количества итераций

#### Теоретическая часть задачи
Важный для анализа графа потока вариант - *упорядочивание в глубину* (depth-first ordering), которое представляет собой обращение обратного порядка обхода. Иначе говоря, при упорядочивании в глубину мы посещаем узел, затем обходим его крайний слева узел-преемник, после этого - узел, затем обходим его крайний справа узел-преемник, после этого - узел, расположенный слева от него, и т.д. Однако, перед тем как строить дерево для графа потока, следует выбрать, какой из преемников является крайним справа, какой - его левым соседом и т.д. 

<img src="https://image.ibb.co/eU6oqy/9bebbf5e_2639_409f_a626_2c10d48cdc83.jpg" width="540" height="600" />

#### Практическая часть задачи (реализация)
```csharp
    // Методы расширения для удобного использования нумерации базовых блоков
    public static class GraphNumExt
    {
        // Обращает порядок нумерации
        public static IGraphNumerator Reverse(this GraphNumerator n, ControlFlowGraph g)
            => new ReverseNum(g, n);

        // Нумерация в обратном порядке
        public static NumeratedGraph BackOrder(this TACode code)
        {
            var graph = new NumeratedGraph(code, null);
            graph.Numerator = GraphNumerator.BackOrder(graph);
            return graph;
        }

        // Нумерация в прямом порядке
        public static NumeratedGraph StraightOrder(this TACode code)
        {
            var graph = new NumeratedGraph(code, null);
            graph.Numerator = GraphNumerator.BackOrder(graph).Reverse(graph);
            return graph;
        }

        // Реализация IGraphNumerator для обращения порядка нумерации 
        private class ReverseNum : IGraphNumerator
        {
            private readonly ControlFlowGraph _graph;
            private readonly IGraphNumerator _num;

            public ReverseNum(ControlFlowGraph graph, IGraphNumerator num)
            {
                _graph = graph;
                _num = num;
            }

            public int? GetIndex(BasicBlock b)
            {
                var ind = _num.GetIndex(b);
                if (ind == null) return null;

                return _graph.CFGNodes.Count() - ind.Value;
            }
        }

    }

    // Реализация нумерации в обратном порядке
    public class GraphNumerator : IGraphNumerator
    {
        public static GraphNumerator BackOrder(ControlFlowGraph graph)
        {
            var root = graph.CFGNodes.ElementAt(0);
            var num = new GraphNumerator();
            var index = 0;
            var openSet = new HashSet<BasicBlock>();

            void Iter(BasicBlock node)
            {
                openSet.Add(node);
                var children = node.Children;
                foreach(var c in children.Where(x => !openSet.Contains(x)))
                    Iter(c);
                num._num[node] = index++;
            }

            Iter(root);
            return num;
        }

        private readonly Dictionary<BasicBlock, int> _num = new Dictionary<BasicBlock, int>();

        public virtual int? GetIndex(BasicBlock b)
            => _num.TryGetValue(b, out var res) ?
                new int?(res) : null;
    }

    // Граф с нумерованными базовыми блоками
    public class NumeratedGraph : ControlFlowGraph
    {
        public NumeratedGraph(TACode code, IGraphNumerator numerator) : base(code)
        {
            Numerator = numerator;
        }

        public IGraphNumerator Numerator { get; set;  }

        public int? IndexOf(BasicBlock b) => Numerator.GetIndex(b);

        private string NodeToString(BasicBlock n)
        {
            var blockName = TACodeNameManager.Instance[n.BlockId];
            var index = Numerator?.GetIndex(n);

            return $"({index}:{blockName})";
        }

        public override string ToString()
        {
            var s = new StringBuilder();

            foreach (var n in CFGNodes)
                s.AppendLine(
                        $"{NodeToString(n)} : [{ String.Join(", ", n.Children.Select(NodeToString)) }]"
                    );
            return s.ToString();
        }
    }

    // Интефейс для любых нумераций CFG
    public interface IGraphNumerator
    {
        int? GetIndex(BasicBlock b);
    }
```

#### Тесты
```csharp
a = 1;
goto h;
h: b = 1;
goto h2;
h2: c = 1;
d = 1;
```
```csharp
numer[0] == cfg[0]
numer[1] == cfg[1]
numer[2] == cfg[2]
```
где `numer` - нумератор, `cfg` - граф потока управления. 
Результат работы - перенумерованные блоки от 0 до 2 сверху вниз.  

<img src="https://image.ibb.co/fcMP0y/1.png" width="100" height="300" />

#### Пример работы
```csharp
var cfg = new ControlFlowGraph(tacodeInstance);
var numer = GraphNumerator.BackOrder(cfg);
Assert.AreEqual(0, numer.GetIndex(cfg.CFGNodes.ElementAt(0)));
Assert.AreEqual(1, numer.GetIndex(cfg.CFGNodes.ElementAt(1)));
Assert.AreEqual(2, numer.GetIndex(cfg.CFGNodes.ElementAt(2)));
```
