using System;

namespace xBot.App
{
	/// <summary>
	/// Abstraction for UI state notifications from bot, proxy, and network layers.
	/// </summary>
	public interface IUiNotifier
	{
		void NotifyProxyState(bool running);
		void NotifyBotState(bool isBotting);
		void NotifyCharacterSelected(string characterName);
		void NotifyTitleUpdated();
	}

	/// <summary>
	/// Null implementation for headless/testing environments.
	/// </summary>
	public class NullUiNotifier : IUiNotifier
	{
		public void NotifyProxyState(bool running) { }
		public void NotifyBotState(bool isBotting) { }
		public void NotifyCharacterSelected(string characterName) { }
		public void NotifyTitleUpdated() { }
	}

	/// <summary>
	/// Global UI notification dispatcher.
	/// </summary>
	public static class UiNotifier
	{
		private static IUiNotifier _instance;

		public static IUiNotifier Instance
		{
			get => _instance ?? (_instance = DefaultNotifier);
			set => _instance = value ?? DefaultNotifier;
		}

		public static IUiNotifier DefaultNotifier { get; set; } = new NullUiNotifier();

		public static void NotifyProxyState(bool running) => Instance.NotifyProxyState(running);
		public static void NotifyBotState(bool isBotting) => Instance.NotifyBotState(isBotting);
		public static void NotifyCharacterSelected(string characterName) => Instance.NotifyCharacterSelected(characterName);
		public static void NotifyTitleUpdated() => Instance.NotifyTitleUpdated();
	}
}
