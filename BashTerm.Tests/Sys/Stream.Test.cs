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
	public void SeqManipulatorBasic() {
		PipeStream<byte> pipe = new(16384);
		var writer = new TextStreamWriter(pipe.CreateWriter());
		var parser = new TextStreamParser(pipe.CreateReader());

		writer.Manip.EraseLine();
		writer.TryWriteLine("what");

		parser.Parse();
		TextStreamToken tk;

		Assert.IsTrue(parser.Get(out tk));
		Assert.IsInstanceOfType<TokenEraseLine>(tk);

		Assert.IsTrue(parser.Get(out tk));
		Assert.IsInstanceOfType<TokenText>(tk);
		Assert.AreEqual("what", ((TokenText)tk).Text);

		Assert.IsTrue(parser.Get(out tk));
		Assert.IsInstanceOfType<TokenLF>(tk);
	}
}
