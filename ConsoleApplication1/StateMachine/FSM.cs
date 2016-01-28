using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StateMachine
{
    public abstract class State
    {   
        protected StateMachine m_fsm;
        public State(StateMachine fsm)
        {
            m_fsm = fsm;
        }

        public abstract void OnEnter();
        public abstract void OnUpdate();
        public abstract void OnLeave();
    }

    public class StateMachine
    {
        private IDictionary<Type, State> m_statesDict = null;
        private State m_curState = null;
        public StateMachine() { }

        public void AddState(State state)
        {
            if (null == m_statesDict)
                m_statesDict = new Dictionary<Type, State>();
            if(!m_statesDict.ContainsKey(state.GetType()))
                m_statesDict.Add(state.GetType(), state);
        }

        public void ChangeState(Type stateType)
        {
            if (null != m_statesDict && m_statesDict.ContainsKey(stateType))
            {
                if (null != m_curState)
                    m_curState.OnLeave();
                m_curState = m_statesDict[stateType];
                m_curState.OnEnter();
            }
        }

        public void OnUpdate()
        {
            if (null != m_curState)
                m_curState.OnUpdate();
        }

        public void Reset()
        {
            m_statesDict.Clear();
            m_curState = null;
        }
    }
}
