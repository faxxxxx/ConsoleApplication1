using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BehaviorTree
{
    using BehaviorTree.Conditon;

    /// <summary>
    /// Priority selector
    /// </summary>
    class BTNodePrioritySelector : BTNode
    {
        public BTNodePrioritySelector(BTNode parent, ICondition condtion):
            base(parent, condtion)
        {
            m_lastSelectIdx = m_currentSelectIdx = -1;
        }

        protected override bool DoEvaluate(InputParam input)
        {
            IterateChildren((i, node) => {
                if (node.Evaluate(input))
                {
                    m_currentSelectIdx = i;
                    return false;
                }
                return true; 
            });
            return m_currentSelectIdx != -1; 
        }

        protected override NodeState DoTick(InputParam input, OutputParam output)
        {
            NodeState ret = NodeState.Finish;
            if (IsValidIdx(m_currentSelectIdx))
            {
                if (m_currentSelectIdx != m_lastSelectIdx && IsValidIdx(m_lastSelectIdx))
                {
                    BTNode child = GetChildByIdx(m_lastSelectIdx);
                    child.Reset(input);
                }
                m_lastSelectIdx = m_currentSelectIdx;
            }

            BTNode curNode = GetChildByIdx(m_currentSelectIdx);
            if (null != curNode)
            {
                ret = curNode.Tick(input, output);
                if (ret == NodeState.Finish)
                    m_lastSelectIdx = -1;
            }
            return ret;
        }

        protected override void DoReset(InputParam input)
        {
            if (IsValidIdx(m_currentSelectIdx))
                GetChildByIdx(m_currentSelectIdx).Reset(input);
            m_currentSelectIdx = m_lastSelectIdx = -1;
        }

        private int m_lastSelectIdx;
        private int m_currentSelectIdx;
    }

    class BTNodeNonePrioritySelector : BTNodePrioritySelector
    {
        public BTNodeNonePrioritySelector(BTNode parent, ICondition condition)
            : base(parent, condition)
        {

        }

        protected override bool DoEvaluate(InputParam input)
        {
            return base.DoEvaluate(input);
        }
    }

    /// <summary>
    /// Sequence
    /// </summary>
    class BTNodeSequence : BTNode
    {
        private int m_currentSelectIdx;

        public BTNodeSequence(BTNode parent, ICondition condition)
            : base(parent, condition)
        {
            m_currentSelectIdx = -1;
        }

        protected override bool DoEvaluate(InputParam input)
        {
            if(!IsValidIdx(m_currentSelectIdx))
                m_currentSelectIdx = 0;
            if (IsValidIdx(m_currentSelectIdx))
                return GetChildByIdx(m_currentSelectIdx).Evaluate(input);
            return false;
        }

        protected override void DoReset(InputParam input)
        {
            if (IsValidIdx(m_currentSelectIdx))
                GetChildByIdx(m_currentSelectIdx).Reset(input);
            m_currentSelectIdx = -1;
        }

        protected override NodeState DoTick(InputParam input, OutputParam output)
        {
            NodeState ret = NodeState.Finish;
            if (IsValidIdx(m_currentSelectIdx))
            {
                ret = GetChildByIdx(m_currentSelectIdx).Tick(input, output);
                if (ret == NodeState.Finish)
                {
                    m_currentSelectIdx++;
                    //if idx is invalid, means all node finish execution, then this node is finish
                    if (!IsValidIdx(m_currentSelectIdx))
                        return NodeState.Finish;
                    else
                        return NodeState.Executing;
                }
            }
            return ret;
        }
    }

    class BTNodeParallel : BTNode
    {
        public enum FinishOP { OR = 1, AND = 2}
        private FinishOP m_finishCondition;
        private IDictionary<int, NodeState> m_childrenState;

        public BTNodeParallel(BTNode parent, ICondition condition, FinishOP finishCondition=FinishOP.OR)
            : base(parent, condition)
        {
            m_childrenState = new Dictionary<int, NodeState>();
            SetFinishCondition(finishCondition);
        }

        public void SetFinishCondition(FinishOP finishCondition)
        {
            m_finishCondition = finishCondition;
        }

        protected override bool DoEvaluate(InputParam input)
        {
            bool ret = true;
            IterateChildren((i, child) =>
            {
                if (!child.Evaluate(input))
                {
                    ret = false;
                    return false;
                }
                return true;
            });
            return ret;
        }

        protected override void DoReset(InputParam input)
        {
            m_childrenState.Clear();
            IterateChildren((i, child) =>
            {
                child.Reset(input);
                return true;
            });
        }

        protected override NodeState DoTick(InputParam input, OutputParam output)
        {
            int finishCount = 0;
            NodeState ret = NodeState.Executing;
            IterateChildren((i, child) =>
            {
                if (m_childrenState.ContainsKey(i))
                {
                    if(m_childrenState[i] == NodeState.Executing)
                        m_childrenState[i] = child.Tick(input, output);
                }
                else
                    m_childrenState[i] = child.Tick(input, output);
                if (m_childrenState[i] == NodeState.Finish)
                    finishCount++;

                if (m_finishCondition == FinishOP.OR && finishCount > 0)
                {
                    ret = NodeState.Finish;
                    return false;
                }

                if (m_finishCondition == FinishOP.AND && finishCount == GetChildrenCount())
                {
                    ret = NodeState.Finish;
                    return false;
                }

                return true;
            });
            return ret;
        }
    }

    class BTNodeLoop : BTNode
    {
        public BTNodeLoop(BTNode parent, ICondition condition, int loopCount)
            : base(parent, condition)
        {
            m_loopCount = loopCount;
        }

        protected override bool DoEvaluate(InputParam input)
        {
            if (!(m_loopCount > 0 && m_curLoop >= 0 && m_curLoop < m_loopCount))
                return false;

            if (IsValidIdx(0))
                return GetChildByIdx(0).Evaluate(input);
            return false;
        }

        protected override void DoReset(InputParam input)
        {
            if (IsValidIdx(0))
                GetChildByIdx(0).Reset(input);
            m_curLoop = 0;
        }

        protected override NodeState DoTick(InputParam input, OutputParam output)
        {
            NodeState ret = NodeState.Finish;
            if (IsValidIdx(0))
            {
                NodeState childState = GetChildByIdx(0).Tick(input, output);
                if (childState == NodeState.Finish)
                {
                    m_curLoop++;
                    if (m_curLoop != m_loopCount)
                        ret = NodeState.Executing;
                    else
                        m_curLoop = 0;
                }
            }
            return ret;
        }

        private int m_loopCount;
        private int m_curLoop;
    }
}
