### Название задачи

Выделение базовых блоков в трехадресном коде.

#### Постановка задачи

Предоставить возможность получить множество базовых блоков по исходному коду программы в формате трехадресного кода.

#### Зависимости задач в графе задач

* Все оптимизации на уровне базовых блоков
* Построение графа потока управления

#### Теоретическая часть задачи

Базовый блок -- последовательность инструкций или кода, имеющая одну точку входа, одну точку выхода и не содержащая инструкций передачи управления ранее точки выхода. Другими словами, это последовательность инструкций, каждая из которых исполняется тогда и только тогда, когда исполняется первая инструкция из последовательности.

На начало базового блока может указывать одновременно несколько инструкций перехода, конец же блока — либо инструкция передачи управления, либо инструкция, предшествующая переходу.

Базовые блоки являются основной единицей кода, над которой проводятся оптимизации компилятором. Также они являются вершинами в графе потока управления. 

#### Практическая часть задачи (реализация)

* Был реализован класс `BasicBlock`, который представляет собой простую обертку над массивом узлов трехадресного кода. Дополнительная возможность, которые предоставляет класс -- идентификация блока с помощью поля ID.
* Был реализован метод построения списка базовых блоков программы как часть класса TACode. 
* При создании базового блока у команды, входящей в этот базовый блок, обновляется вспомогательное поле `Block`.

Основная работа алгоритма состоит в поиске лидеров. Лидер -- это точка входа в базовый блок.

```algorithm
FindLeaders(CodeList)
    add(leaders, CodeList[0])
    for i = 1 to size(CodeList) do
        node = CodeList[i]

        if node is labeled and i not in leaders then
            add(leaders, i)
        if node is GoTo then
            add(leaders, i+1)
```

По полученному списку лидеров можно сделать разбиение на диапазоны участков кода. Т.е. из набора лидеров вида `[a0, a1, a2, a3, ...]` получается набор лидеров вида `[(a0, a1), (a1, a2), (a2, a3), ...]`. При таком подходе для правильной группировки пар требуется добавить последнюю команду в список лидеров: `add(leaders, size(CodeList))`.

Для каждой полученной пары генерируется базовый блок, в который помещаются все команды из диапазона `[a_i, a_{i+1})`

<!-- #### Тесты

TODO

#### Пример работы

TODO -->
