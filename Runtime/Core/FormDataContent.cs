using System;
using System.IO;
using System.Text;

namespace Kogase.Core
{
    public class FormDataContent
    {
        readonly string boundary;
        readonly MemoryStream stream;
        readonly StreamWriter writer;

        public FormDataContent()
        {
            boundary = "-----------" + Guid.NewGuid().ToString().Replace("-", "");
            stream = new MemoryStream();
            writer = new StreamWriter(stream, Encoding.ASCII);
            writer.Write("--{0}", boundary);
        }

        public FormDataContent Add(string filename, byte[] data)
        {
            writer.Write(
                "\r\nContent-Disposition: form-data; name=\"file\"; filename=\"{0}\"\r\nContent-Type: {1}\r\n\r\n",
                filename,
                "octet/stream");

            writer.Flush();

            writer.BaseStream.Write(data, 0, data.Length);
            writer.Write("\r\n--{0}", boundary);

            return this;
        }

        public FormDataContent Add(string name, string value)
        {
            writer.Write("\r\nContent-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}", name, value);

            writer.Write("\r\n--{0}", boundary);

            writer.Flush();

            return this;
        }

        public string GetMediaType()
        {
            return "multipart/form-data; boundary=" + boundary;
        }

        public byte[] Get()
        {
            writer.Write("--\r\n");
            writer.Flush();

            return stream.ToArray();
        }
    }
}