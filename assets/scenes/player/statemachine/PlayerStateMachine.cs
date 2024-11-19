using System.Collections.Generic;
using Godot;

public partial class PlayerStateMachine : FiniteStateMachine<PlayerState, PlayerController>
{
    protected override PlayerState InitialState => PlayerState.Moving;
    protected override PlayerState NoneState => PlayerState.None;

    Dictionary<PlayerState, State<PlayerState, PlayerController>> states = new Dictionary<
        PlayerState,
        State<PlayerState, PlayerController>
    >()
    {
        [PlayerState.Moving] = new PlayerMovingState(),
        [PlayerState.UsingComputer] = new PlayerUsingComputerState(),
        [PlayerState.UsingPhone] = new PlayerUsingPhoneState(),
        [PlayerState.Sitting] = new PlayerSittingState(),
    };

    protected override Dictionary<PlayerState, State<PlayerState, PlayerController>> States =>
        states;
}
