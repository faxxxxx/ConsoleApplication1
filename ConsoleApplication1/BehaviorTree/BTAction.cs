using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BehaviorTree
{
    using BehaviorTree.Conditon;

    class BTAction : BTNode
    {
        enum BehaviorState
        {
            Ready = 1,
            Running = 2,
            Finish = 3
        }

        private BehaviorState m_behaviorState = BehaviorState.Ready;
        public BTAction(BTNode parent, ICondition condition)
            : base(parent, condition) { }

        protected override void DoReset(InputParam input)
        {
            DoExit(input);
            m_behaviorState = BehaviorState.Ready;
        }

        protected override NodeState DoTick(InputParam input, OutputParam output)
        {
            NodeState ret = NodeState.Finish;
            if (m_behaviorState == BehaviorState.Ready)
            {
                DoEnter(input);
                m_behaviorState = BehaviorState.Running;
            }

            if (m_behaviorState == BehaviorState.Running)
            {
                ret = DoExecute(input, output);
                SetActiveNode(this);
                if (ret == NodeState.Finish)
                    m_behaviorState = BehaviorState.Finish;
            }

            if (m_behaviorState == BehaviorState.Finish)
            {
                DoExit(input);
                m_behaviorState = BehaviorState.Ready;
                SetActiveNode(null);
            }
            return ret;
        }

        protected void DoEnter(InputParam input) { }
        protected NodeState DoExecute(InputParam input, OutputParam output) { return NodeState.Finish; }
        protected void DoExit(InputParam input) { }
    }
}
