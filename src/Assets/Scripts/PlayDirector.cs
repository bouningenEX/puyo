using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayDirector : MonoBehaviour
{
    interface IState
    {
        public enum E_State
        {
            Control = 0,
            GameOver = 1,
            Falling = 2,

            MAX,

            Unchanged,
        }

        E_State Initialize(PlayDirector parent);
        E_State Update(PlayDirector parent);
        
    }

   
    IState.E_State _current_state = IState.E_State.Falling;
    static readonly IState[] states = new IState[(int)IState.E_State.MAX]
    {
        new ControlState(),
        new GameOverState(),
        new FallingState(),
    };

    PlayerController _playerController = null;
[SerializeField] GameObject player = default!;
    LogicalInput _logicalInput = new();
    BoardController _boardController = default!;

    NextQueue _nextQueue = new();
    [SerializeField] PuyoPair[] nextPuyoPairs = {default!,default!};
    // Start is called before the first frame update
    static readonly KeyCode[] key_code_tbl = new KeyCode[(int)LogicalInput.Key.MAX]
    {
        KeyCode.RightArrow,
        KeyCode.LeftArrow,
        KeyCode.X,
        KeyCode.Z,
        KeyCode.UpArrow,
        KeyCode.DownArrow,
    };
    void Start()
    {
        _playerController = player.GetComponent<PlayerController>();
        _logicalInput.Clear();
        _playerController.SetLogicalInput(_logicalInput);
        _playerController.SetLogicalInput(_logicalInput);

        _nextQueue.Initialize();
        InitializeState();
    }
    void UpdateNextView()
    {
        _nextQueue.Each((int idx, Vector2Int n) => { nextPuyoPairs[idx++].SetPuyoType((PuyoType)n.x, (PuyoType)n.y); });
    }
    
void UpdateInput()
    {
        LogicalInput.Key inputDev = 0;
        for (int i = 0; i < (int)LogicalInput.Key.MAX; i++)
        {
            if (Input.GetKey(key_code_tbl[i]))
            {
                inputDev |= (LogicalInput.Key)(1 << i);
            }
        }
        _logicalInput.Update(inputDev);
    }
    void FixedUpdate()
    {
     
        UpdateInput();

        UpdateState();
    }
    

    // Update is called once per frame
    bool Spawn(Vector2Int next) => _playerController.Spawn((PuyoType)next[0], (PuyoType)next[1]);

    class ControlState : IState
    {
       public IState.E_State Initialize(PlayDirector parent)
        {
            if (!parent.Spawn(parent._nextQueue.Update()))
            {
                return IState.E_State.GameOver;
            }
            parent.UpdateNextView();
            return IState.E_State.Unchanged;
        }

        public IState.E_State Update(PlayDirector parent)
        {
            return parent.player.activeSelf ? IState.E_State.Unchanged : IState.E_State.Falling;
        }
    }
    class GameOverState :IState
    {
        IState.E_State IState.Initialize(PlayDirector parent)
        {
            SceneManager.LoadScene(0);
            return IState.E_State.GameOver;
        }
        IState.E_State IState.Update(PlayDirector parent) { return IState.E_State.Unchanged; }
    }
    class FallingState : IState
    {
        public IState.E_State Initialize(PlayDirector parent)
        {
            return parent._boardController.CheckFall() ? IState.E_State.Unchanged : IState.E_State.Control;
        }
        public IState.E_State Update(PlayDirector parent)
        {
            return parent._boardController.Fall() ? IState.E_State.Unchanged : IState.E_State.Control;
        }

    }
    void InitializeState()
    {
        Debug.Assert(condition: _current_state is >= 0 and < IState.E_State.MAX);

        var next_state = states[(int)_current_state].Initialize(this);

        if(next_state !=IState.E_State.Unchanged)
        {
            _current_state = next_state;
            InitializeState();
        }
    }

    void UpdateState()
    {
        Debug.Assert(condition:_current_state is >= 0 and < IState.E_State.MAX);

        var next_state =states[(int)_current_state].Update(this);
        if(next_state!=IState.E_State.Unchanged)
        {
            _current_state=next_state;
            InitializeState();
        }
    }
}
