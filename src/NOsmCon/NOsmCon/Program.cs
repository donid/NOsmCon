using System.Globalization;

using CommandLine;

using OsmSharp.Streams;

namespace NOsmCon;

internal class Program
{

	private static void Main(string[] args)
	{
		Console.WriteLine("NOsmCon");

		ParserResult<object> res = Parser.Default.ParseArguments<ExtractCommandLineOptions, MergeCommandLineOptions>(args);
		if (res.Value is ExtractCommandLineOptions extractOptions)
		{
			ExecuteExtract(extractOptions);
		}
		else if (res.Value is MergeCommandLineOptions mergeOptions)
		{
			ExecuteMerge(mergeOptions);
		}

	}

	private static void ExecuteMerge(MergeCommandLineOptions options)
	{
		//todo
		Console.WriteLine("Not implemented!");
	}

	private static void ExecuteExtract(ExtractCommandLineOptions options)
	{
		Console.WriteLine("Extracting...");

		string sourceFilePath = options.SourceFile;
		string outFilePath = options.OutputFile;
		OsmFileFormat outputFormat = options.OutputFormat;
		bool completeWays = options.CompleteWays;
		string[] boundingBoxParts = options.BoundingBox.Split(',');
		float[] boundingBoxFloats = boundingBoxParts.Select(x => float.Parse(x, CultureInfo.InvariantCulture)).ToArray();
		float bBoxLeft = Math.Min(boundingBoxFloats[0], boundingBoxFloats[1]);
		float bBoxRight = Math.Max(boundingBoxFloats[0], boundingBoxFloats[1]);
		float bBoxBottom = Math.Min(boundingBoxFloats[2], boundingBoxFloats[3]);
		float bBoxTop = Math.Max(boundingBoxFloats[2], boundingBoxFloats[3]);

		Console.WriteLine($"SourceFile: '{sourceFilePath}'");
		Console.WriteLine($"OutputFile: '{outFilePath}'");
		Console.WriteLine($"OutputFormat: '{outputFormat}'");
		Console.WriteLine($"CompleteWays: '{completeWays}'");
		Console.WriteLine($"BoundingBox left right: {ToInvariantString(bBoxLeft)} - {ToInvariantString(bBoxRight)}");
		Console.WriteLine($"BoundingBox bottom top: {ToInvariantString(bBoxBottom)} - {ToInvariantString(bBoxTop)}");

		FileInfo sourceFile = new FileInfo(sourceFilePath);
		OsmFileFormat sourceFormat = sourceFile.Extension.ToLower() == ".pbf" ? OsmFileFormat.pbf : OsmFileFormat.xml;
		Console.WriteLine($"SourceFormat (from file-extension): '{sourceFormat}'");


		if (!sourceFile.Exists)
		{
			Console.WriteLine("SourceFile not found!");
			return;
		}
		FileInfo outFile = new FileInfo(outFilePath);

		FileStream sourceStream = sourceFile.OpenRead();
		OsmStreamSource source = CreateStreamSource(sourceStream, sourceFormat);

		var filtered = source.FilterBox(bBoxLeft, bBoxTop, bBoxRight, bBoxBottom, completeWays);

		using (var outStream = outFile.Open(FileMode.Create, FileAccess.ReadWrite))
		{
			OsmStreamTarget target = CreateStreamTarget(outStream, outputFormat);
			target.RegisterSource(filtered);
			target.Pull(); // ATTENTION: Nuget throws MissingMethodException in protobuf
						   // but referencing V:\projects_os_git\OsmSharp_core\src\OsmSharp\bin\Debug\netstandard2.0\OsmSharp.dll works
		}
		Console.WriteLine("Extract completed!");
	}
	private static string ToInvariantString(float bBoxLeft)
	{
		return bBoxLeft.ToString(CultureInfo.InvariantCulture);
	}

	private static OsmStreamTarget CreateStreamTarget(FileStream stream, OsmFileFormat format)
	{
		if (format == OsmFileFormat.xml)
		{
			return new XmlOsmStreamTarget(stream);
		}
		else
		{
			return new PBFOsmStreamTarget(stream);
		}
	}
	private static OsmStreamSource CreateStreamSource(FileStream stream, OsmFileFormat format)
	{
		if (format == OsmFileFormat.xml)
		{
			return new XmlOsmStreamSource(stream);
		}
		else
		{
			return new PBFOsmStreamSource(stream);
		}
	}

}

