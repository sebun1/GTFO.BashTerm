using System.Security.Cryptography;
using Bsh.Sys.Stream;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bsh.Tests.Sys;

[TestClass]
public class PipeStreamTest {
	[TestMethod]
	public void SingularReaderWriter() {
		PipeStream<byte> pipe = new();
		var writer = pipe.CreateWriter();
		var reader = pipe.CreateReader();

		Assert.ThrowsException<InvalidOperationException>(() => pipe.CreateWriter());
		Assert.ThrowsException<InvalidOperationException>(() => pipe.CreateWriter());
	}

	[TestMethod]
	public void ByteWriterReadWrite() {
		Span<byte> data = stackalloc byte[256];
		RandomNumberGenerator.Fill(data);

		PipeStream<byte> pipe = new();
		var writer = pipe.CreateWriter();
		var reader = pipe.CreateReader();
	}
}
