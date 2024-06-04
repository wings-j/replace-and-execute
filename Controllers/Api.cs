using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AspNetCore.Mvc;
using replace_and_execute.Types;
using System.Diagnostics;

namespace replace_and_execute.Controllers
{
    /// <summary>
    /// API
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class Api : ControllerBase
    {
        readonly IConfiguration configuration;

        /// <summary>
        /// Constructor
        /// </summary>
        public Api(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// ExecudeCommand
        /// </summary>
        /// <param name="commands">Commands</param>
        private static List<string> ExecuteCommand(string[] commands, string cwd)
        {
            List<string> outputs = new()
            {
                "[Commands Execution Start]"
            };
            var process = Process.Start(new ProcessStartInfo(commands[0], commands[1] ?? "") { RedirectStandardOutput = true, WorkingDirectory = cwd });
            if(process == null)
            {
                outputs.Add("[Commands Execution Failed]");
            }
            else
            {
                using(var reader = process.StandardOutput)
                {
                    while(!reader.EndOfStream)
                    {
                        var s = reader.ReadLine();
                        if(s != null)
                        {
                            outputs.Add(s);
                        }
                    }
                    if(!process.HasExited)
                    {
                        process.Kill();
                    }
                }
                outputs.Add($"[Commands Execution Failed with Code: {process.ExitCode}]");
            }

            return outputs;
        }

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="file">File</param>
        /// <returns>Action Result</returns>
        [HttpPost("Update")]
        public async Task<ActionResult<List<string>>> PostUpdate([FromForm] string name, [FromForm] IFormFile file)
        {
            var module = configuration.GetSection("modules").Get<List<Module>>()?.Find((a) => a.Name == name);
            if(module != null)
            {
                List<string> outputs = new();

                if(module.Pre.Length > 0)
                {
                    outputs.Add("[Pre Command Found]");
                    outputs.AddRange(ExecuteCommand(module.Pre, module.Path));
                }

                var tempFilePath = Path.Combine(Path.GetTempPath(), file.FileName);
                using(var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                };

                using(var zipInputStream = new ZipInputStream(file.OpenReadStream()))
                {
                    ZipEntry entry;
                    while((entry = zipInputStream.GetNextEntry()) != null)
                    {
                        var directoryName = Path.GetDirectoryName(entry.Name);
                        var fileName = Path.GetFileName(entry.Name);
                        var directoryFullPath = Path.Combine(module.Path, directoryName ?? "");
                        var fileFullPath = Path.Combine(module.Path, entry.Name);
                        if(!Directory.Exists(directoryFullPath))
                        {
                            Directory.CreateDirectory(directoryFullPath);
                        }
                        if(!string.IsNullOrEmpty(fileName))
                        {
                            using var streamWriter = System.IO.File.Create(fileFullPath);
                            zipInputStream.CopyTo(streamWriter);
                        }
                    }
                }

                if(module.Post.Length > 0)
                {
                    outputs.Add("[Post Command Found]");
                    outputs.AddRange(ExecuteCommand(module.Post, module.Path));
                }

                return Ok(outputs);
            }
            else
            {
                return NotFound();
            }
        }
    }
}
