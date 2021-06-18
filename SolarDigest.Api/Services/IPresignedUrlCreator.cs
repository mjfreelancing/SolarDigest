﻿using System.Threading.Tasks;

namespace SolarDigest.Api.Functions
{
    public interface IPresignedUrlCreator
    {
        // userSecretPath refers to the root path in the parameter store where the access/secret keys are stored
        Task<string> CreateDownloadUrlAsync(string name);
        Task<string> CreateUploadUrlAsync(string name);
    }
}