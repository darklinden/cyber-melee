//
public class LockstepRoutes : RouteBase
{

    public Cmd TimeSync = new Cmd("time.sync", typeof(Proto.RequestTimeSync), typeof(Proto.ResponseTimeSync));
    public Cmd PlayerEnter = new Cmd("player.enter", typeof(Proto.RequestEnter), typeof(Proto.ResponseEnter));
    public Cmd BattleStartBroadcast = new Cmd("battle.start.broadcast", null, typeof(Proto.ServerBroadcastBattleStart));
    public Cmd BattleLoadProgressPush = new Cmd("battle.load.progress.push", typeof(Proto.ClientPushBattleLoadProgress), null);
    public Cmd BattleLoadProgressBroadcast = new Cmd("battle.load.progress.broadcast", null, typeof(Proto.ServerBroadcastBattleLoadProgress));
    public Cmd BattleStartedBroadcast = new Cmd("battle.started.broadcast", null, typeof(Proto.ServerBroadcastBattleStarted));
    public Cmd BattleActionPush = new Cmd("battle.action.push", typeof(Proto.ClientPushBattleAction), null);
    public Cmd BattleActionBroadcast = new Cmd("battle.action.broadcast", null, typeof(Proto.ServerBroadcastBattleAction));
    public Cmd BattleEnd = new Cmd("battle.end", typeof(Proto.RequestBattleEnd), typeof(Proto.ResponseBattleEnd));
    public Cmd BattleShouldFinishBroadcast = new Cmd("battle.should.finish.broadcast", null, typeof(Proto.ServerBroadcastBattleShouldFinish));
    public Cmd BattleFinishedBroadcast = new Cmd("battle.finished.broadcast", null, typeof(Proto.ServerBroadcastBattleFinished));
    public Cmd BattleReconnect = new Cmd("battle.reconnect", typeof(Proto.RequestReconnect), typeof(Proto.ResponseReconnect));
    public Cmd BattleReconnectedGameState = new Cmd("battle.reconnected.state", null, typeof(Proto.ReconnectedBattleState));
}