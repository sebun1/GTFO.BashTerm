using Bsh.Sys.Render.Parser;
using Bsh.Sys.Stream;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bsh.Tests.Sys;

[TestClass]
public class StreamTest {
	[TestMethod]
	public void SingularReaderWriter() {
		PipeStream<byte> pipe = new();
		var writer = pipe.CreateWriter();
		var reader = pipe.CreateReader();

		Assert.ThrowsException<InvalidOperationException>(() => pipe.CreateWriter());
		Assert.ThrowsException<InvalidOperationException>(() => pipe.CreateWriter());
	}

	[TestMethod]
	public void ByteReadWrite() {
		var rng = new Random(12345);
		byte[] data = new byte[1024];
		rng.NextBytes(data);

		PipeStream<byte> pipe = new();
		var writer = pipe.CreateWriter();
		var reader = pipe.CreateReader();

		for (var i = 0; i < data.Length; i++) {
			Assert.IsTrue(writer.TryWrite(data[i]));
			bool got = reader.TryRead(out var b);
			Assert.IsTrue(got);
			Assert.AreEqual(data[i], b);
		}

		writer.TryWriteMultiple(data.ToArray());
		byte[] readBuff = new byte[data.Length];
		bool res = reader.TryReadMultiple(data.Length, readBuff);
		Assert.IsTrue(res);
		Assert.IsTrue(data.SequenceEqual(readBuff));
	}

	[TestMethod]
	public void TextReadWrite() {
		string str = "你好，世界！Hello, World!";

		PipeStream<byte> pipe = new(16384);
		var writer = new TextStreamWriter(pipe.CreateWriter());
		var reader = new TextStreamReader(pipe.CreateReader());

		Assert.IsTrue(writer.TryWriteLine(str));
		Assert.IsTrue(reader.TryReadLine(out string line));
		Assert.AreEqual(str, line);

		string word0 = "word1231231";
		Assert.IsTrue(writer.TryWrite($"{word0} word2 word3"));
		Assert.IsTrue(reader.TryReadWord(out string word));
		Assert.AreEqual(word0, word);
	}

	[TestMethod]
	public void SeqManipPerformance() {
		PipeStream<byte> pipe = new(0);
		System.Diagnostics.Stopwatch sw = new();
		var writer = new TextStreamWriter(pipe.CreateWriter());
		var reader = new TextStreamParser(pipe.CreateReader());

		const int samples = 5000000;

		reader.SetParseLimit(samples + 200);

		sw.Start();
		for (int i = 0; i < samples; i++) {
			writer.Manip.SetColor(255, 0, 0);
		}

		sw.Stop();
		Console.WriteLine(
			$"SetColor\tWrite\ttotal:{sw.ElapsedMilliseconds}\tavg:{sw.ElapsedMilliseconds * 1.0 / samples}");

		sw.Restart();
		reader.Parse();
		sw.Stop();
		Console.WriteLine(
			$"SetColor\tRead\ttotal:{sw.ElapsedMilliseconds}\tavg:{sw.ElapsedMilliseconds * 1.0 / samples}");

		sw.Restart();
		for (int i = 0; i < samples; i++) {
			writer.Manip.EraseLine();
		}

		sw.Stop();
		Console.WriteLine(
			$"EraseLine\tWrite\ttotal:{sw.ElapsedMilliseconds}\tavg:{sw.ElapsedMilliseconds * 1.0 / samples}");

		sw.Restart();
		reader.Parse();
		sw.Stop();
		Console.WriteLine(
			$"EraseLine\tRead\ttotal:{sw.ElapsedMilliseconds}\tavg:{sw.ElapsedMilliseconds * 1.0 / samples}");
	}

	[TestMethod]
	public void SeqManipulatorBasic() {
		PipeStream<byte> pipe = new(16384);
		var writer = new TextStreamWriter(pipe.CreateWriter());
		var parser = new TextStreamParser(pipe.CreateReader());

		writer.Manip.EraseLine();
		writer.Manip.SetColor(255, 0, 0);
		writer.Manip.SetColor(255, 127, 255, false);
		writer.Manip.SetColor(new(255, 127, 255));
		writer.Manip.SetColor("FF7FFF", false);
		writer.TryWriteLine("what");
		writer.Manip.SetCursor(1, 2);
		writer.Manip.SetCursor(65535, 65534);

		parser.Parse();
		TextStreamToken? tk;

		Assert.IsTrue(parser.Get(out tk));
		Assert.IsInstanceOfType<TxtTokenEraseLine>(tk);

		Assert.IsTrue(parser.Get(out tk));
		Assert.IsInstanceOfType<TxtTokenSetFgColor>(tk);
		Assert.AreEqual(255, ((TxtTokenSetFgColor)tk).R);
		Assert.AreEqual(0, ((TxtTokenSetFgColor)tk).G);
		Assert.AreEqual(0, ((TxtTokenSetFgColor)tk).B);

		Assert.IsTrue(parser.Get(out tk));
		Assert.IsInstanceOfType<TxtTokenSetBgColor>(tk);
		Assert.AreEqual(255, ((TxtTokenSetBgColor)tk).R);
		Assert.AreEqual(127, ((TxtTokenSetBgColor)tk).G);
		Assert.AreEqual(255, ((TxtTokenSetBgColor)tk).B);

		Assert.IsTrue(parser.Get(out tk));
		Assert.IsInstanceOfType<TxtTokenSetFgColor>(tk);
		Assert.AreEqual(255, ((TxtTokenSetFgColor)tk).R);
		Assert.AreEqual(127, ((TxtTokenSetFgColor)tk).G);
		Assert.AreEqual(255, ((TxtTokenSetFgColor)tk).B);

		Assert.IsTrue(parser.Get(out tk));
		Assert.IsInstanceOfType<TxtTokenSetBgColor>(tk);
		Assert.AreEqual(255, ((TxtTokenSetBgColor)tk).R);
		Assert.AreEqual(127, ((TxtTokenSetBgColor)tk).G);
		Assert.AreEqual(255, ((TxtTokenSetBgColor)tk).B);

		Assert.IsTrue(parser.Get(out tk));
		Assert.IsInstanceOfType<TxtTokenText>(tk);
		Assert.AreEqual("what", ((TxtTokenText)tk).Text);

		Assert.IsTrue(parser.Get(out tk));
		Assert.IsInstanceOfType<TxtTokenLF>(tk);

		Assert.IsTrue(parser.Get(out tk));
		Assert.IsInstanceOfType<TxtTokenSetCursor>(tk);
		Assert.AreEqual(1, ((TxtTokenSetCursor)tk).X);
		Assert.AreEqual(2, ((TxtTokenSetCursor)tk).Y);

		Assert.IsTrue(parser.Get(out tk));
		Assert.IsInstanceOfType<TxtTokenSetCursor>(tk);
		Assert.AreEqual(65535, ((TxtTokenSetCursor)tk).X);
		Assert.AreEqual(65534, ((TxtTokenSetCursor)tk).Y);
	}

	[TestMethod]
	public void SeqManipulatorExtended() {
		PipeStream<byte> pipe = new(0);
		var writer = new TextStreamWriter(pipe.CreateWriter());
		var parser = new TextStreamParser(pipe.CreateReader());

		// Cursor Movement
		writer.Manip.MoveCursorUp(5);
		writer.Manip.MoveCursorDown(5);
		writer.Manip.MoveCursorRight(5);
		writer.Manip.MoveCursorLeft(5);
		writer.Manip.MoveCursorStartOfNextLine(5);
		writer.Manip.MoveCursorStartOfPrevLine(5);
		writer.Manip.MoveCursorToColumn(10);
		writer.Manip.SetCursorHome();

		// Erase
		writer.Manip.Erase2ScreenEnd();
		writer.Manip.Erase2ScreenStart();
		writer.Manip.EraseScreen();
		writer.Manip.Erase2LineEnd();
		writer.Manip.Erase2LineStart();
		writer.Manip.EraseLine();

		// Styles & Graphics
		writer.Manip.UnsetFgColor();
		writer.Manip.UnsetBgColor();
		writer.Manip.SetBold();
		writer.Manip.UnsetBold();
		writer.Manip.SetItalic();
		writer.Manip.UnsetItalic();
		writer.Manip.SetUnderline();
		writer.Manip.UnsetUnderline();
		writer.Manip.SetStrikethrough();
		writer.Manip.UnsetStrikethrough();
		writer.Manip.ResetStyles();

		parser.Parse();
		TextStreamToken? tk;

		// Cursor Movement Asserts
		Assert.IsTrue(parser.Get(out tk));
		Assert.IsInstanceOfType<TxtTokenMoveCursor>(tk);
		Assert.AreEqual(0, ((TxtTokenMoveCursor)tk).X);
		Assert.AreEqual(-5, ((TxtTokenMoveCursor)tk).Y);

		Assert.IsTrue(parser.Get(out tk));
		Assert.IsInstanceOfType<TxtTokenMoveCursor>(tk);
		Assert.AreEqual(0, ((TxtTokenMoveCursor)tk).X);
		Assert.AreEqual(5, ((TxtTokenMoveCursor)tk).Y);

		Assert.IsTrue(parser.Get(out tk));
		Assert.IsInstanceOfType<TxtTokenMoveCursor>(tk);
		Assert.AreEqual(5, ((TxtTokenMoveCursor)tk).X);
		Assert.AreEqual(0, ((TxtTokenMoveCursor)tk).Y);

		Assert.IsTrue(parser.Get(out tk));
		Assert.IsInstanceOfType<TxtTokenMoveCursor>(tk);
		Assert.AreEqual(-5, ((TxtTokenMoveCursor)tk).X);
		Assert.AreEqual(0, ((TxtTokenMoveCursor)tk).Y);

		Assert.IsTrue(parser.Get(out tk));
		Assert.IsInstanceOfType<TxtTokenMoveCursorStartOfLine>(tk);
		Assert.AreEqual(5, ((TxtTokenMoveCursorStartOfLine)tk).offset);

		Assert.IsTrue(parser.Get(out tk));
		Assert.IsInstanceOfType<TxtTokenMoveCursorStartOfLine>(tk);
		Assert.AreEqual(-5, ((TxtTokenMoveCursorStartOfLine)tk).offset);

		Assert.IsTrue(parser.Get(out tk));
		Assert.IsInstanceOfType<TxtTokenSetCursorColumn>(tk);
		Assert.AreEqual(10, ((TxtTokenSetCursorColumn)tk).X);

		Assert.IsTrue(parser.Get(out tk));
		Assert.IsInstanceOfType<TxtTokenSetCursor>(tk);
		Assert.AreEqual(0, ((TxtTokenSetCursor)tk).X);
		Assert.AreEqual(0, ((TxtTokenSetCursor)tk).Y);

		// Erase Asserts
		Assert.IsTrue(parser.Get(out tk));
		Assert.IsInstanceOfType<TxtTokenEraseToEnd>(tk);

		Assert.IsTrue(parser.Get(out tk));
		Assert.IsInstanceOfType<TxtTokenEraseToStart>(tk);

		Assert.IsTrue(parser.Get(out tk));
		Assert.IsInstanceOfType<TxtTokenEraseScreen>(tk);

		Assert.IsTrue(parser.Get(out tk));
		Assert.IsInstanceOfType<TxtTokenEraseToLineEnd>(tk);

		Assert.IsTrue(parser.Get(out tk));
		Assert.IsInstanceOfType<TxtTokenEraseToLineStart>(tk);

		Assert.IsTrue(parser.Get(out tk));
		Assert.IsInstanceOfType<TxtTokenEraseLine>(tk);

		// Style Asserts
		Assert.IsTrue(parser.Get(out tk));
		Assert.IsInstanceOfType<TxtTokenUnsetFgColor>(tk);

		Assert.IsTrue(parser.Get(out tk));
		Assert.IsInstanceOfType<TxtTokenUnsetBgColor>(tk);

		Assert.IsTrue(parser.Get(out tk));
		Assert.IsInstanceOfType<TxtTokenSetBold>(tk);

		Assert.IsTrue(parser.Get(out tk));
		Assert.IsInstanceOfType<TxtTokenUnsetBold>(tk);

		Assert.IsTrue(parser.Get(out tk));
		Assert.IsInstanceOfType<TxtTokenSetItalic>(tk);

		Assert.IsTrue(parser.Get(out tk));
		Assert.IsInstanceOfType<TxtTokenUnsetItalic>(tk);

		Assert.IsTrue(parser.Get(out tk));
		Assert.IsInstanceOfType<TxtTokenSetUnderline>(tk);

		Assert.IsTrue(parser.Get(out tk));
		Assert.IsInstanceOfType<TxtTokenUnsetUnderline>(tk);

		Assert.IsTrue(parser.Get(out tk));
		Assert.IsInstanceOfType<TxtTokenSetStrikethrough>(tk);

		Assert.IsTrue(parser.Get(out tk));
		Assert.IsInstanceOfType<TxtTokenUnsetStrikethrough>(tk);

		Assert.IsTrue(parser.Get(out tk));
		Assert.IsInstanceOfType<TxtTokenResetStyles>(tk);

		// No more tokens expected
		Assert.IsFalse(parser.Get(out tk));
	}
}
