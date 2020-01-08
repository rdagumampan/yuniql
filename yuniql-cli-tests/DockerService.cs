using Docker.DotNet;
using System;
using System.Runtime.InteropServices;
using Docker.DotNet.Models;
using System.Collections.Generic;
using System.Linq;

namespace Yuniql.CliTests
{
    public class DockerService : IDisposable
    {
        private DockerClient _dockerClient;

        public void Initialize()
        {
            var dockerApiUri = new Uri(GetDockerApiUri());
            _dockerClient = new DockerClientConfiguration(dockerApiUri).CreateClient();
        }

        public string GetDockerApiUri()
        {
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            if (isWindows)
            {
                return "npipe://./pipe/docker_engine";
            }

            var isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            if (isLinux)
            {
                return "unix:/var/run/docker.sock";
            }

            throw new Exception("Was unable to determine what OS this is running on, does not appear to be Windows or Linux!?");
        }

        public void Pull(DockerImage image)
        {
            _dockerClient.Images
                .CreateImageAsync(new ImagesCreateParameters
                {
                    FromImage = image.Image,
                    Tag = string.IsNullOrEmpty(image.Tag) ? "latest" : image.Tag
                },
                new AuthConfig(),
                new Progress<JSONMessage>())
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        public string Run(DockerContainer container)
        {
            var portBindings = new Dictionary<string, IList<PortBinding>>();
            container.MappedPorts.ForEach(portMap =>
            {
                portBindings.Add(portMap.Item1, new List<PortBinding> { new PortBinding { HostIP = "localhost", HostPort = portMap.Item2 } });
            });

            var response = _dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters
            {
                Name = container.Name,
                Image = $"{container.Image}:{container.Tag}",
                Env = container.Env.Select(t => $"{t.Item1}={t.Item2}").ToList(),
                Cmd = container.Cmd.Select(t => $"{t.Item1}={t.Item2}").ToList(),
                ExposedPorts = container.ExposedPorts.ToDictionary(x => x, x => default(EmptyStruct)),
                HostConfig = new HostConfig
                {
                    PortBindings = portBindings,
                    PublishAllPorts = true
                }
            })
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

            _dockerClient.Containers.StartContainerAsync(response.ID, null)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

            return response.ID;
        }
        public void Start(DockerContainer container)
        {
            _dockerClient.Containers.StartContainerAsync(container.Id, new ContainerStartParameters())
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        public void Stop(DockerContainer container)
        {
            _dockerClient.Containers.StopContainerAsync(container.Id, new ContainerStopParameters())
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        public void Remove(DockerContainer container)
        {
            _dockerClient.Containers.RemoveContainerAsync(container.Id, new ContainerRemoveParameters { Force = true })
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        public List<DockerContainer> FindByName(string containerName)
        {
            var containers = _dockerClient.Containers.ListContainersAsync(new ContainersListParameters { All = true })
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult()
                .Where(r => r.Names.Contains(containerName))
                .Select(r => new DockerContainer { Id = r.ID, Image = r.Image, State = r.State })
                .ToList();

            return containers;
        }

        public DockerContainer FindById(string containerId)
        {
            var container = _dockerClient.Containers.ListContainersAsync(new ContainersListParameters { All = true })
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult()
                .Where(r => r.ID == containerId)
                .Select(r => new DockerContainer { Id = r.ID, Image = r.Image, State = r.State })
                .FirstOrDefault();

            return container;
        }

        private bool _disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _dockerClient.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}