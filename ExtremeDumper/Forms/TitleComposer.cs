using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtremeDumper.Forms;

sealed class TitleComposer {
	static readonly char[] uppercaseLetters = "ÅBĊĎĘḞĢΉÎĴĶḼḾŅÕҎQŖŜTỰṼẂẌẎẐ".ToCharArray();
	static readonly char[] lowercaseLetters = "åbċďęḟģήîĵķḽḿņõҏqŗŝtựṽẃẍẏẑ".ToCharArray();

	public string Title { get; set; } = string.Empty;

	public string? Subtitle { get; set; }

	public string? Version { get; set; }

	public IDictionary<string, string?> Annotations { get; } = new Dictionary<string, string?>();

	public string Compose(bool obfuscate) {
		var sb = new StringBuilder(Title);
		if (!string.IsNullOrEmpty(Subtitle)) {
			sb.Append(" - ");
			sb.Append(Subtitle);
		}
		if (!string.IsNullOrEmpty(Version)) {
			sb.Append(" ");
			sb.Append(Version);
		}
		var annotations = Annotations.Values.Where(t => !string.IsNullOrEmpty(t)).ToArray();
		if (annotations.Length != 0) {
			sb.Append(" (");
			sb.Append(string.Join(", ", annotations));
			sb.Append(')');
		}
		var result = sb.ToString();
		if (obfuscate)
			result = Obfuscate(result);
		return result;
	}

	public static string Compose(bool obfuscate, string title, string? subtitle, string? version, params string?[]? annotations) {
		var titleManager = new TitleComposer {
			Title = title,
			Subtitle = subtitle,
			Version = version
		};
		if (annotations is not null) {
			for (int i = 0; i < annotations.Length; i++) {
				if (!string.IsNullOrEmpty(annotations[i]))
					titleManager.Annotations.Add(i.ToString(), annotations[i]);
			}
		}
		return titleManager.Compose(obfuscate);
	}

	static string Obfuscate(string title) {
		var random = new Random();
		var sb = new StringBuilder(title.Length * 2);
		for (int i = 0; i < title.Length - 1; i++) {
			char c = title[i];
			if ('A' <= c && c <= 'Z')
				sb.Append(uppercaseLetters[c - 'A']);
			else if ('a' <= c && c <= 'z')
				sb.Append(lowercaseLetters[c - 'a']);
			else
				sb.Append(c);
		}
		sb.Append(title[title.Length - 1]);
		return sb.ToString();
	}
}
