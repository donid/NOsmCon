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

        if (string.IsNullOrEmpty(options.BoundingBox))
        {
            Console.WriteLine("The BoundingBox argument is null or empty");
            return;
        }
        string parseError = ParseBoundingBox(options.BoundingBox, out float[] boundingBoxFloats);
		if (parseError != "")
		{
			Console.WriteLine(parseError);
			return;
		}
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
		DirectoryInfo? outDirectory = outFile.Directory;
		if (outDirectory == null || outDirectory.Exists == false)
		{
			Console.WriteLine("Directory for OutputFile not found!");
			return;
		}

		FileStream sourceStream = sourceFile.OpenRead();
		OsmStreamSource source = CreateStreamSource(sourceStream, sourceFormat);

		var filtered = source.FilterBox(bBoxLeft, bBoxTop, bBoxRight, bBoxBottom, completeWays);

		FileStream outStream;
		try
		{
			outStream = outFile.Open(FileMode.Create, FileAccess.ReadWrite);
		}
		catch (Exception ex)
		{
			Console.WriteLine("Error creating OutputFile: " + ex.Message);
			return;

		}

		using (outStream)
		{
			OsmStreamTarget target = CreateStreamTarget(outStream, outputFormat);
			target.RegisterSource(filtered);
			target.Pull(); // ATTENTION: Nuget throws MissingMethodException in protobuf
						   // but referencing V:\projects_os_git\OsmSharp_core\src\OsmSharp\bin\Debug\netstandard2.0\OsmSharp.dll works
		}
		Console.WriteLine("Extract completed!");
	}

	private static string ParseBoundingBox(string boundingBox, out float[] parsedValues)
	{
		string[] boundingBoxParts = boundingBox.Split(',');
		if (boundingBoxParts.Length != 4)
		{
			parsedValues = new float[0];
			return "The BoundingBox argument does not contain four comma-separated values.";
		}
		parsedValues = new float[4];
		for (int index = 0; index < boundingBoxParts.Length; index++)
		{
			float range = index < 2 ? 180 : 90;
			string errorMessage = ParseBoundingBoxValue(boundingBoxParts[index], range, out float value);
			if (errorMessage != "")
			{
				return $"The BoundingBox argument value #{index + 1} {errorMessage}.";
			}
			parsedValues[index] = value;
		}
		return "";
	}

	private static string ParseBoundingBoxValue(string stringValue, float range, out float value)
	{
		if (float.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedValue))
		{
			value = parsedValue;
			if (value >= range || value <= -range)
			{
				return "does not fall in the range +/-" + range;
			}
			return "";
		}
		value = 0f;
		return "cannot be parsed as float";
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

