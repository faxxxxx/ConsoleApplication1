using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BehaviorTree
{
    using BehaviorTree.Conditon;

    enum NodeState
    {
        Executing = 1,
        Finish = 2,
        ErrorTransition = -1
    }

    public class InputParam
    {

    }

    public class OutputParam
    {

    }

    public class IBTNode
    {
        bool Evaluate(InputParam input) { return DoEvaluate(input); }
        bool DoEvaluate(InputParam input) { return true; }
    }

    class BTNode
    {
        private IList<BTNode> m_childrens = null;
        private BTNode m_parentNode = null;
        private ICondition m_condtion = null;
        private BTNode m_lastActiveNode = null;
        private BTNode m_activeNode = null;
        private String m_name = "";

        public BTNode(BTNode parentNode, ICondition condition = null)
        {
            _SetParentNode(parentNode);
            SetCondition(condition ?? new ConditionTRUE());
        }

        public void SetCondition(ICondition condition) { m_condtion = condition; }
        protected bool IsValidIdx(int idx) { return idx >= 0 && idx < m_childrens.Count(); }
        protected int GetChildrenCount() { return m_childrens.Count(); }

        public void SetName(String name) { m_name = name; }
        public String GetName() { return m_name; }

        public bool Evaluate(InputParam input) { return (m_condtion != null && m_condtion.Check(input)) && DoEvaluate(input); }
        public NodeState Tick(InputParam input, OutputParam output) { return DoTick(input, output); }
        public void Reset(InputParam input) { DoReset(input); }

        protected virtual void DoReset(InputParam input) { }
        protected virtual NodeState DoTick(InputParam input, OutputParam output) { return NodeState.Finish; }
        protected virtual bool DoEvaluate(InputParam input) { return true; }

        private void _SetParentNode(BTNode parentNode) { m_parentNode = parentNode; }

        protected void SetActiveNode(BTNode node)
        {
            m_lastActiveNode = m_activeNode;
            m_activeNode = node;
            if (null != m_parentNode)
                m_parentNode.SetActiveNode(node);
        }

        public BTNode AddChildNode(BTNode child)
        {
            if (null == m_childrens)
                m_childrens = new List<BTNode>();

            if (!m_childrens.Contains(child))
                m_childrens.Add(child);
            return this;
        }

        public BTNode RemoveChildNode(BTNode child)
        {
            if (null != m_childrens && null != child)
                m_childrens.Remove(child);
            return this;
        }

        public delegate bool IterCallback(int i, BTNode node);
        protected void IterateChildren(IterCallback callback)
        {
            int len = m_childrens.Count();
            for (int i = 0; i < len; i++)
            {
                if (!callback(i, m_childrens[i]))
                    break;
            }
        }

        protected BTNode GetChildByIdx(int idx)
        {
            BTNode ret = null;
            if (IsValidIdx(idx))
                ret = m_childrens[idx];
            return ret;
        }
    }
}
