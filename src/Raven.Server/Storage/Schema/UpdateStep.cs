﻿using Raven.Server.Documents;
using Voron.Impl;

namespace Raven.Server.Storage.Schema
{
    public class UpdateStep
    {
        public Transaction ReadTx;
        public Transaction WriteTx;
        public ConfigurationStorage ConfigurationStorage;
        public DocumentsStorage DocumentsStorage;
    }
}
