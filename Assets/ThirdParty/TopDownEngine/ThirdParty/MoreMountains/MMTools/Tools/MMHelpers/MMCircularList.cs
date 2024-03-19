using System.Collections.Generic;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 구문을 분석하고 끝이나 시작에 도달하면 자동으로 시작이나 끝으로 반복되도록 하는 개선된 목록
    /// 사용하려면: CurrentIndex를 원하는 대로 설정한 다음 IncrementCurrentIndex / DecrementCurrentIndex를 사용하여 이동하고 Current를 통해 현재 요소를 가져옵니다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MMCircularList<T> : List<T>
	{
		private int _currentIndex = 0;
		
		/// <summary>
		/// Lets you set the current index, or compute it if you get it
		/// </summary>
		public int CurrentIndex
		{
			get
			{
				return GetCurrentIndex();
			}
			set => _currentIndex = value;
		}

		/// <summary>
		/// Computes the current index
		/// </summary>
		/// <returns></returns>
		protected virtual int GetCurrentIndex()
		{
			if (_currentIndex > Count - 1) { _currentIndex = 0; }
			if (_currentIndex < 0) { _currentIndex = Count - 1; }
			return _currentIndex;
		}

		/// <summary>
		/// Returns the current element
		/// </summary>
		public T Current => this[CurrentIndex];

		/// <summary>
		/// Increments the current index (towards the "right" of the list)
		/// </summary>
		public virtual void IncrementCurrentIndex()
		{
			_currentIndex++;
			GetCurrentIndex();
		}

		/// <summary>
		/// Decrements the current index (towards the "left" of the list)
		/// </summary>
		public virtual void DecrementCurrentIndex()
		{
			_currentIndex--;
			GetCurrentIndex();
		}

		/// <summary>
		/// Returns the previous index in the circular list
		/// </summary>
		public virtual int PreviousIndex => (_currentIndex == 0) ? Count - 1 : _currentIndex - 1;

		/// <summary>
		/// Returns the next index in the circular list
		/// </summary>
		public virtual int NextIndex => (_currentIndex == Count - 1) ? 0 : _currentIndex + 1;
	}
}

