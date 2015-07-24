using System;
using System.IO;
using System.Text;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;

namespace live.asp.net.TagHelpers
{
    internal class TagHelperOutputTextWriter : TextWriter
    {
        private readonly Encoding _encoding;
        private readonly TagHelperOutput _output;

        public TagHelperOutputTextWriter(TagHelperOutput output)
            : this(output, Encoding.UTF8)
        {

        }

        public TagHelperOutputTextWriter(TagHelperOutput output, Encoding encoding)
        {
            _output = output;
            _encoding = encoding;
        }

        public override Encoding Encoding => _encoding;

        public override void Write(string value)
        {
            _output.Content.Append(value);
        }

        public override void Write(string format, params object[] args)
        {
            _output.Content.AppendFormat(format, args);
        }

        public override void Write(char value)
        {
            _output.Content.Append(value.ToString());
        }
    }
}