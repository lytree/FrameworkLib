﻿
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Framework.System.Text.Json.Serialization;

public class DateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
{
	private readonly string _format = "yyyy-MM-dd HH:mm:ss";
	public DateTimeOffsetJsonConverter()
	{

	}

	public DateTimeOffsetJsonConverter(string format)
	{
		_format = format;
	}

	public override void Write(Utf8JsonWriter writer, DateTimeOffset date, JsonSerializerOptions options)
	{
		writer.WriteStringValue(date.ToString(_format));
	}

	public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var value = reader.GetString();

		if (string.IsNullOrEmpty(value))
			return DateTimeOffset.MinValue;

		if (DateTimeOffset.TryParse(value, out DateTimeOffset res))
			return res;

		if (DateTimeOffset.TryParse(value, null, DateTimeStyles.AssumeUniversal, out res))
			return res;

		if (DateTimeOffset.TryParse(value, null, DateTimeStyles.RoundtripKind, out res))
			return res;

		return DateTimeOffset.ParseExact(value, _format, null);
	}
}
