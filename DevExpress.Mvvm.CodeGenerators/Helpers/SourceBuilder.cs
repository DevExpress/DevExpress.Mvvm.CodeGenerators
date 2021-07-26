﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DevExpress.Mvvm.CodeGenerators {
    public class SourceBuilder {
        class NewLineState {
            public int? LastTabLevel;
        }

        public readonly SourceBuilder Return;
        readonly StringBuilder builder;
        readonly int tabs;
        readonly NewLineState newLineState;
        int? LastTabLevel { get => newLineState.LastTabLevel; set => newLineState.LastTabLevel = value; }
        SourceBuilder tab;

        public SourceBuilder Tab {
            get {
                if(tab != null)
                    return tab;
                return tab = new SourceBuilder(builder, tabs + 1, this, newLineState);
            }
        }

        public SourceBuilder(StringBuilder builder) 
            : this(builder, 0, null, new NewLineState()) {
        }
        SourceBuilder(StringBuilder builder, int tabs, SourceBuilder @return, NewLineState newLineState) {
            this.builder = builder;
            this.tabs = tabs;
            Return = @return;
            this.newLineState = newLineState;
        }

        public SourceBuilder Append(string str) {
            BeforeAppend();
            builder.Append(str);
            return this;
        }
        public SourceBuilder Append(char character) {
            BeforeAppend();
            builder.Append(character);
            return this;
        }
        public SourceBuilder Append(string str, int statIndex, int count) {
            BeforeAppend();
            builder.Append(str, statIndex, count);
            return this;
        }
        void BeforeAppend() {
            if(LastTabLevel != null) {
                if(LastTabLevel != tabs)
                    throw new InvalidOperationException();
                return;
            }
            LastTabLevel = tabs;
            for(int i = 0; i < tabs; i++) {
                builder.Append("    ");
            }
        }
        public SourceBuilder AppendLine() {
            LastTabLevel = null;
            builder.Append(Environment.NewLine);
            return this;
        }
    }

    public static class SourceBuilderExtensions {
        public static SourceBuilder AppendLine(this SourceBuilder builder, string str) => builder.Append(str).AppendLine();

        public static void AppendMultipleLines(this SourceBuilder builder, string lines) {
            foreach((int start, int length) in new LineEnumerator(lines)) {
                builder.Append(lines, start, length).AppendLine();
            }
        }

        public struct LineEnumerator
        {
            readonly string lines;
            int startIndex;
            public (int start, int length) Current { get; private set; }

            public LineEnumerator(string source) {
                lines = source;
                Current = default;
                startIndex = 0;
            }
            public LineEnumerator GetEnumerator() {
                return this;
            }
            public bool MoveNext() {
                if(startIndex == lines.Length) return false;
                var index = lines.IndexOf("\r\n", startIndex);
                if(index != -1) {
                    Current = (startIndex, index - startIndex);
                    startIndex = index + 2; ;
                    return true;
                } else {
                    Current = (startIndex, lines.Length - startIndex);
                    startIndex = lines.Length;
                    return true;
                }
            }
        }

        public static SourceBuilder AppendFirstToUpperCase(this SourceBuilder builder, string str) {
            return builder.AppendChangeFirstCore(str, char.ToUpper(str[0]));
        }
        public static SourceBuilder AppendFirstToLowerCase(this SourceBuilder builder, string str) {
            return builder.AppendChangeFirstCore(str, char.ToLower(str[0]));
        }
        static SourceBuilder AppendChangeFirstCore(this SourceBuilder builder, string str, char firstChar) {
            return builder.Append(firstChar).Append(str, 1, str.Length - 1);
        }
        public static void AppendMultipleLinesWithSeparator(this SourceBuilder builder, IEnumerable<string> lines, string separator) {
            bool appendSeparator = false;
            foreach(var line in lines) {
                if(appendSeparator)
                    builder.Append(separator);
                builder.Append(line);
                appendSeparator = true;
            }
        }
    }
}