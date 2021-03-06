### Название задачи
Итерационный алгоритм для достигающих определений.

#### Постановка задачи
Реалзовать итерационный алгоритм для достигающих определений.

#### Зависимости задач в графе задач
Зависит от:
* Интерфейс передаточной функции
* Передаточная функция и генерация множеств gen и kill
* Стурктура базовых блоков
От задачи зависит:
* Тестирование итерационного алгоритма

#### Теоретическая часть задачи
**Определение:** Будем говорить, что определение d достигает точки p, если существует путь от точки, непосредственно следующей за d, к точке p, такой, что d не уничтожается вдоль этого пути.

Анализ должен быть консервативным: если не знаем, есть ли другое присваивание на пути, то считаем, что существует.  
Достигающие определения используются при:
1. Является ли x константой в точке p? (если p достигает одно определение x, и это – определение константы)
2. Является ли x в точке p неинициализированной? (если p не достигает ни одно определение x)

Передаточная функция в общем случае для достигающих определений:  
<img src="https://image.ibb.co/fSvrOJ/IMG_30052018_211321_0.jpg" width="540" height="100" />

Оператор сбора для достигающих определений:   
<img src="https://image.ibb.co/gL9J3J/IMG_30052018_211511_0.jpg" width="220" height="200" />

Уравнения для достигающих определений:  
<img src="https://image.ibb.co/jeuqHd/IMG_30052018_211623_0.jpg" width="300" height="180" />

Итеративный алгоритм:  
**Вход**: граф потока управления, в котором для каждого ББл вычислены genB и killB  
**Выход**: Множества достигающих определений на входе IN[B] и на выходе OUT[B] для каждого ББл B  
  
<img src="https://image.ibb.co/hRkxxd/IMG_30052018_211654_0.jpg" width="540" height="180" />

**Сходимость алгоритма**: на каждом шаге IN[B] и OUT[B] не уменьшаются для всех B и ограничены сверху, поэтому алгоритм сходится.

  
#### Практическая часть задачи (реализация)
Алгоритм реализован в соответствии со схемой, приведенной выше.
```csharp
public class IterativeAlgorithm : IAlgorithm<HashSet<Guid>>
{
    public InOutData<HashSet<Guid>> Analyze(ControlFlowGraph graph, ILatticeOperations<HashSet<Guid>> ops, ITransferFunction<HashSet<Guid>> f)
    {
        var data = new InOutData<HashSet<Guid>>
        {
            [graph.CFGNodes.ElementAt(0)] = (ops.Lower, ops.Lower)
        };
        foreach (var node in graph.CFGNodes)
            data[node] = (ops.Lower, ops.Lower);
        var outChanged = true;
        while (outChanged)
        {
            outChanged = false;
            foreach (var block in graph.CFGNodes)
            {
                var inset = block.Parents.Aggregate(ops.Lower, (x, y)
                    => ops.Operator(x, data[y].Item2));
                var outset = f.Transfer(block, inset, ops);
                if (outset.Except(data[block].Item2).Any())
                {
                    outChanged = true;
                    data[block] = (inset, outset);
                }
            }
        }
        return data;
    }
}
```

#### Тесты

```csharp
l1: a = 3 - 5
l2: b = 10 + 2
l3: c = -1
l_: if 1 goto l3
ass4 = l5: d = c + 1999
l_: if 2 goto l2
l7: e = 7 * 4
l8: f = 100 / 25
```

```csharp
IN[0] = { }
OUT[0] = { l1 }

IN[1] = { l1, l2, l3, l5 }
OUT[1] = { l1, l2, l3, l4 }

IN[2] = { l1, l2, l3, l5 },
OUT[2] = { l1, l2, l4, l5 }

IN[3] =  { l1, l2, l3, l5 }
OUT[3] =  { l1, l2, l3, l5 }

IN[4] =  { l1, l2, l3, l5 }
OUT[4] = {  l1, l2, l3, l5, l7, l8 }
```

#### Пример работы.
![](https://image.ibb.co/hjczy8/Capture.png)  
<img src="https://image.ibb.co/hAb6OJ/IMG_30052018_213638_0.jpg" width="540" height="140" />