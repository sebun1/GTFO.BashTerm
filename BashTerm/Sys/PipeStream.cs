using System.Text;
using BashTerm.Exec;
using BashTerm.Utils;
using UnityEngine;

namespace BashTerm.Sys;

public class PipeStream {
	public readonly int StreamId;
	private readonly StringBuilder _writeCache = new();
	private string _readCache;
	private readonly Queue<string> _stringBuffer = new();
	private readonly Queue<PipeObject> _objectBuffer = new();
	private int _readHead = 0;
	private int _referenceCount = 0;
	private readonly char[] _whitespaceChars = new[] { ' ', '\t', '\n', '\r' };
	private readonly char[] _newlineChars = new[] { '\n', '\r' };
	public bool IsClosedForWriting { get; private set; }
	public bool IsScreen {
		get {
			return ReadingScreen != null;
		}
	}
	public Screen? ReadingScreen { get; private set; }

	public PipeStream(int streamId) {
		StreamId = streamId;
		_readCache = string.Empty;
		IsClosedForWriting = false;
		ReadingScreen = null;
	}

	public void SetReadingScreen(Screen? readingScreen) {
		ReadingScreen = readingScreen;
	}

	public bool HasReferences {
		get {
			return _referenceCount > 0;
		}
	}

	internal void AddReference() {
		_referenceCount++;
	}

	internal bool ReleaseReference() {
		if (_referenceCount == 0) return false;
		_referenceCount--;
		return true;
	}

	// Write

	public void Write(string data) {
		if (IsClosedForWriting) {
			Logr.Warn($"PipeStream[{StreamId}] is closed for writing, cannot write data (str segment): {data}");
			return;
		}
		if (_writeCache.Length > 0) {
			_writeCache.Append(data);
			data = _writeCache.ToString();
			_writeCache.Clear();
		}
		_stringBuffer.Enqueue(data);
	}

	public void Write(PipeObject obj) {
		if (IsClosedForWriting) {
			Logr.Warn($"PipeStream[{StreamId}] is closed for writing, cannot write data (object): {obj}");
			return;
		}
		_objectBuffer.Enqueue(obj);
	}

	public void Print(string data) {
		if (IsClosedForWriting) {
			Logr.Warn($"PipeStream[{StreamId}] is closed for writing, cannot write data (print): {data}");
			return;
		}
		_writeCache.Append(data);
	}

	public void Println(string data) {
		if (IsClosedForWriting) {
			Logr.Warn($"PipeStream[{StreamId}] is closed for writing, cannot write data (println): {data}");
			return;
		}
		_writeCache.Append(data + '\n');
	}

	public void Flush() {
		if (_writeCache.Length == 0) return;
		_stringBuffer.Enqueue(_writeCache.ToString());
		_writeCache.Clear();
	}

	// Availability

	public bool Available() {
		return StringAvailable() || ObjectAvailable();
	}

	public bool StringAvailable() {
		VerifyCache();
		return _stringBuffer.Count > 0 || _readCache.Length - _readHead > 0;
	}

	public bool ObjectAvailable() {
		return _objectBuffer.Count > 0;
	}

	// Read

	/// <summary>
	/// Read the next character in the buffer
	/// </summary>
	/// <param name="c"></param>
	/// <returns></returns>
	public bool ReadChar(out char c) {
		VerifyCache();
		if (_readCache.Length == 0) {
			while (_readCache.Length == 0) {
				if (_stringBuffer.Count == 0) {
					c = '\0';
					return false;
				}

				ResetCache();
				_readCache += _stringBuffer.Dequeue();
			}
		}
		c = _readCache[_readHead];
		_readHead++;
		return true;
	}

	/// <summary>
	/// Seek the buffer for a non-whitespace char, then
	/// read until a whitespace char is encountered
	/// Consumes the whitespace character
	/// </summary>
	/// <param name="word"></param>
	/// <returns></returns>
	public bool ReadWord(out string word) {
		VerifyCache();
		int tmpHead = _readHead;
		// empty buffer
		if (_readCache.Length == 0 && _stringBuffer.Count == 0) {
			word = string.Empty;
			return false;
		}

		int end = _readCache.IndexOfAny(_whitespaceChars, tmpHead);
		while (end < 0 || end <= tmpHead) {
			// whitespace before any read character
			if (tmpHead == end && tmpHead + 1 < _readCache.Length) {
				tmpHead++;
				end = _readCache.IndexOfAny(_whitespaceChars, tmpHead);
				continue;
			}

			if (_stringBuffer.Count == 0) {
				word = string.Empty;
				return false;
			}

			_readCache += _stringBuffer.Dequeue();
			end = _readCache.IndexOfAny(_whitespaceChars, tmpHead);
		}

		word = _readCache.Substring(tmpHead, end - tmpHead);
		_readHead = end + 1;
		return true;
	}

	/// <summary>
	/// Read the string buffer until a newline is encountered
	/// Consumes the newline character
	/// </summary>
	/// <param name="line"></param>
	/// <returns></returns>
	public bool ReadLine(out string line) {
		VerifyCache();
		int tmpHead = _readHead;
		if (_readCache.Length == 0 && _stringBuffer.Count == 0) {
			line = string.Empty;
			return false;
		}

		int end = _readCache.IndexOfAny(_newlineChars, tmpHead);
		while (end < 0) {
			if (_stringBuffer.Count == 0) {
				line = string.Empty;
				return false;
			}

			_readCache += _stringBuffer.Dequeue();
			end = _readCache.IndexOfAny(_newlineChars, tmpHead);
		}

		line = _readCache.Substring(tmpHead, end - tmpHead);
		_readHead = end + 1;
		return true;
	}

	/// <summary>
	/// Reads until the end of the current segment
	/// </summary>
	/// <returns></returns>
	public bool ReadSegment(out string? seg) {
		VerifyCache();
		if (_readCache.Length == 0)
			return _stringBuffer.TryDequeue(out seg);
		seg = _readCache.Substring(_readHead, _readCache.Length - _readHead);
		ResetCache();
		return true;
	}

	/// <summary>
	/// Reads the next object in the queue
	/// </summary>
	/// <param name="obj"></param>
	/// <returns>true if an object is available</returns>
	public bool ReadObject(out PipeObject? obj) {
		return _objectBuffer.TryDequeue(out obj);
	}

	private void VerifyCache() {
		if (_readHead >= _readCache.Length) {
			ResetCache();
		}
	}

	private void ResetCache() {
		_readCache = "";
		_readHead = 0;
	}


	// Close
	public void Close() {
		Flush();
		IsClosedForWriting = true;
	}
}
