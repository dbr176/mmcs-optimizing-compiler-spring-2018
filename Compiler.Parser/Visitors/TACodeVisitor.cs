﻿﻿using System;
using Compiler.Parser.AST;
using Compiler.ThreeAddressCode;
using System.Collections.Generic;

namespace Compiler.Parser.Visitors
{
    public class TACodeVisitor : AutoVisitor
    {
        /// <summary>
        /// Программа в виде списка команд трехадреного кода
        /// </summary>
        private TACode code = new TACode();

        /// <summary>
        /// Список помеченных команд(строк) трехадресного кода в формате ИмяМетки - Узел
        /// </summary>
        private Dictionary<string, TA_Node> labeledTANodes = new Dictionary<string, TA_Node>();
        
        /// <summary>
        /// Список команд безусловного перехода, ведущих в непреобразованную часть кода и ожидающих заполнения поля TargetLabel, в формате ИмяМетки - Список команд перехода к этой метке
        /// </summary>
        private Dictionary<string, List<TA_Goto>> forwardGotos = new Dictionary<string, List<TA_Goto>>();

        /// <summary>
        /// Список переменных исходного кода в формате ИмяПеременной - Адрес в трехадресном коде
        /// </summary>
        private Dictionary<string, TA_Var> m_varsInCode = new Dictionary<string, TA_Var>();

        public override void VisitLabelNode(LabelNode l)
        {
            string labelName = l.Label.Name;
            // Создаем пустой оператор и указываем, что на него есть переход по метке
            TA_Empty labeledNop = GetEmptyLabeledNode();

            // Добавляем метку и помеченный оператор в список помеченных операторов (это всегда нужно делать, т.к. дальше по тексту могут оказаться goto на данную метку)
            labeledTANodes.Add(labelName, labeledNop);

            // Проверяем, не было ли по этой метке переходов, преобразованных ранее
            if (forwardGotos.ContainsKey(labelName))
            {
                // Если были, заполняем их поля меток и удаляем из списка ожидания
                foreach (var ta_goto in forwardGotos[labelName])
                    ta_goto.TargetLabel = labeledNop.Label;
                forwardGotos.Remove(labelName);
            }

            // Продолжаем отдельно разбор помеченного оператора как обычного
            l.Stat.Visit(this);
        }

        public override void VisitGoToNode(GoToNode g)
        {
            string labelName = g.Label.Name;
            // При посещении узла GoTо создаем соответсвующую команду трехадресного кода
            var gt = new TA_Goto();
            code.AddNode(gt);

            // Если метка ведет в уже преобразованную часть программы
            if (labeledTANodes.ContainsKey(labelName))
            {
                // Получаем помеченную строку треадресного кода и задаем ее как цель перехода
                TA_Node target = labeledTANodes[labelName];
                gt.TargetLabel = target.Label;
            }
            else
            {
                // Иначе помещаем строку в лист ожидания пока помеченная часть программы не будет преобразована
                if (!forwardGotos.ContainsKey(labelName))
                    forwardGotos.Add(labelName, new List<TA_Goto>());
                forwardGotos[labelName].Add(gt);
            }
        }

        public override void VisitAssignNode(AssignNode a)
        {
            var assign = new TA_Assign();
            assign.Left = null;
            assign.Right = RecAssign(a.Expr);
            assign.Result = GetVarByName(a.Id.Name);
            assign.Operation = OpCode.TA_Copy;

            code.AddNode(assign);
        }

        // TODO
        public override void VisitCycleNode(CycleNode c)
        {
            base.VisitCycleNode(c);
        }

        // TODO
        public override void VisitBlockNode(BlockNode bl)
        {
            base.VisitBlockNode(bl);
        }
        
        public override void VisitPrintNode(PrintNode pr)
        {
            TA_Print print = null;
            foreach (var expr in pr.ExprList.ExpList)
            {
                print = new TA_Print();
                print.Data = RecAssign(expr);
                print.Sep = " ";
                code.AddNode(print);
            }

            if (print != null)
                print.Sep = Environment.NewLine;
        }

        // TODO
        public override void VisitExprListNode(ExprListNode exn)
        {
            base.VisitExprListNode(exn);
        }

        // TODO
        public override void VisitIfNode(IfNode iif)
        {
            var igt = new TA_IfGoto();
            // Результат вычисления логического выражения
            TA_Var cond = RecAssign(iif.Expr);
            igt.Condition = cond;
            // Добавление новой метки непосредственно перед телом условного оператора 
            TA_Empty newLabel = GetEmptyLabeledNode();
            igt.TargetLabel = newLabel.Label;
            // Обход выражений тела условного оператора
            iif.Stat1.Visit(this);
            iif.Stat2.Visit(this);
            code.AddNode(igt);
        }

        // TODO
        public override void VisitForNode(ForNode f)
        {
            base.VisitForNode(f);
        }

        public override void VisitEmptyNode(EmptyNode w)
        {
            code.AddNode(new TA_Empty());
        }
        
        /// <summary>
        /// Рекурсивный разбор выражений и генерация их кода
        /// </summary>
        private TA_Var RecAssign(ExprNode ex)
        {
            var assign = new TA_Assign();
            var result = new TA_Var();
            assign.Result = result;

            // Обход продолжается до тех пор, пока выражение не окажется переменной или константой
            switch (ex)
            {
                case IdNode tmp1:
                    assign.Left = null;
                    assign.Right = GetVarByName(tmp1.Name);
                    assign.Operation = OpCode.TA_Copy;
                    break;

                case IntNumNode tmp2:
                    assign.Left = null;
                    assign.Right = GetConst(tmp2.Num);
                    assign.Operation = OpCode.TA_Copy;
                    break;

                case BinaryNode tmp3:
                    assign.Left = RecAssign(tmp3.Left);
                    assign.Right = RecAssign(tmp3.Right);
                    if (Enum.TryParse(tmp3.Operation, out OpCode op1))
                        assign.Operation = op1;
                    break;

                case UnaryNode tmp4:
                    assign.Left = null;
                    assign.Right = RecAssign(tmp4.Num);
                    if (Enum.TryParse(tmp4.Operation.ToString(), out OpCode op2))
                        assign.Operation = op2;
                    break;
            }

            code.AddNode(assign);
            return result;
        }
        
        /// <summary>
        /// Создать новый пустой оператор - метку в ТА коде
        /// </summary>
        private TA_Empty GetEmptyLabeledNode()
        {
            var labeledNop = new TA_Empty {IsLabeled = true};
            code.AddNode(labeledNop);
            return labeledNop;
        }

        /// <summary>
        /// Найти переменную по имени в исходном коде
        /// </summary>
        private TA_Var GetVarByName(string name)
        {
            if (!m_varsInCode.ContainsKey(name))
                m_varsInCode.Add(name, new TA_Var());

            return m_varsInCode[name];
        }

        /// <summary>
        /// Создать константу
        /// </summary>
        private TA_IntConst GetConst(int value)
        {
            return new TA_IntConst(value);
        }
    }
}