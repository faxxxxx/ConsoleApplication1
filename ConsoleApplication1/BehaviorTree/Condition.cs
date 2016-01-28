using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace BehaviorTree
{
    namespace Conditon
    {
        public interface ICondition
        {
            bool Check(InputParam input);
        }

        public class ConditionTRUE : ICondition
        {
            public bool Check(InputParam input) { return true; }
        }

        public class ConditionFALSE : ICondition
        {
            public bool Check(InputParam input) { return false; }
        }

        public class CondtionNOT : ICondition
        {
            private ICondition m_innerCondition;
            public CondtionNOT(ICondition condition)
            {
                Debug.Assert(condition != null, "create CondtionNot failed!");
                m_innerCondition = condition;
            }

            public bool Check(InputParam input)
            {
                return !m_innerCondition.Check(input);
            }
        }

        public abstract class ConditionGroup : ICondition
        {
            protected IList<ICondition> m_conditions = new List<ICondition>();
            protected void AddCondtion(ICondition conditon) 
            {
                if (m_conditions.IndexOf(conditon) == -1)
                    m_conditions.Add(conditon);
            }

            public virtual bool Check(InputParam input) { return true; }
        }

        public class ConditionAND : ConditionGroup
        {
            public ConditionAND(ICondition lhsCondtion, ICondition rhsCondition)
            {
                AddCondtion(lhsCondtion);
                AddCondtion(rhsCondition);
            }

            public override bool Check(InputParam input)
            {
                foreach (ICondition condition in m_conditions)
                {
                    if (!condition.Check(input))
                        return false;
                }
                return true;
            }
        }

        public class ConditionOR : ConditionGroup
        {
            public ConditionOR(ICondition lhsCondtion, ICondition rhsCondition)
            {
                AddCondtion(lhsCondtion);
                AddCondtion(rhsCondition);
            }

            public override bool Check(InputParam input)
            {
                foreach (ICondition condition in m_conditions)
                {
                    if (condition.Check(input))
                        return true;
                }
                return false;
            }
        }

        public class ConditionXOR : ConditionGroup
        {
            public ConditionXOR(ICondition lhsCondition, ICondition rhsCondition)
            {
                AddCondtion(lhsCondition);
                AddCondtion(rhsCondition);
            }

            public override bool Check(InputParam input)
            {
                bool ret = true;
                foreach (ICondition condition in m_conditions)
                    ret ^= condition.Check(input);
                return ret;
            }
        }

        public class ConditionDelegate : ICondition
        {
            public delegate bool CheckDelegate(InputParam input);
            private CheckDelegate m_delegate = null;
            public ConditionDelegate(CheckDelegate checkDelegate)
            {
                m_delegate = checkDelegate;
            }

            public virtual bool Check(InputParam input)
            {
                if (null != m_delegate)
                    return m_delegate(input);
                return false;
            }
        }
    }

}
