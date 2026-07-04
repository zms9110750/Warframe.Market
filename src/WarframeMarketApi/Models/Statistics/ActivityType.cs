namespace zms9110750.WarframeMarketApi.Models.Statistics;

/// <summary>
/// 统计相关的辅助类型：活动类型
/// </summary>
public enum ActivityType
{
	/// <summary>未知</summary>
	Unknown,
	/// <summary>空闲</summary>
	Idle,
	/// <summary>任务中</summary>
	OnMission,
	/// <summary>道场中</summary>
	InDojo,
	/// <summary>轨道飞行器中</summary>
	InOrbiter,
	/// <summary>中继站中</summary>
	InRelay,
}
