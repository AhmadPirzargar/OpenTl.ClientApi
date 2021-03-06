﻿namespace OpenTl.ClientApi.MtProto.UnitTests.Layers.Messages
{
    using System;
    using System.Threading.Tasks;

    using DotNetty.Transport.Channels.Embedded;

    using Moq;

    using OpenTl.ClientApi.MtProto.Layers.Messages.Adapters;
    using OpenTl.ClientApi.MtProto.Services.Interfaces;
    using OpenTl.ClientApi.MtProto.UnitTests.Framework;
    using OpenTl.ClientApi.MtProto.UnitTests.Framework.Builders;
    using OpenTl.Common.Extensions;
    using OpenTl.Common.Testing;
    using OpenTl.Schema;

    using Xunit;

    public sealed class BadServerSaltHandlerTest: UnitTest
    {
        private static readonly Random Random = new Random();
        
        [Fact]
        public void BadServerSaltHandle()
        {
            this.RegisterType<BadServerSaltHandler>();

            var mSettings =  this.BuildClientSettingsProps();
            
            var requestEncoder = this.Resolve<BadServerSaltHandler>();
            
            var channel = new EmbeddedChannel(requestEncoder);

            var badServerSalt = new TBadServerSalt
                                    {
                                        
                                        NewServerSalt = Random.NextLong()
                                    };

            // ---

            channel.WriteInbound(badServerSalt);

            // ---
            
            Assert.Null(channel.ReadOutbound<object>());
            Assert.Equal(BitConverter.GetBytes(badServerSalt.NewServerSalt), mSettings.Object.ClientSession.ServerSalt);
        }
        
        
        [Fact]
        public async Task RepeatSendMessage()
        {
            this.RegisterType<BadServerSaltHandler>();

            this.BuildClientSettingsProps();

            const int BadMsgId = 1;
            var request = new RequestPing();

            this.Resolve<Mock<ISessionWriter>>()
                .BuildSuccessSave();
            
            var mRequestService = this.Resolve<Mock<IRequestService>>();

            mRequestService.Setup(rs => rs.GetRequestToReply(BadMsgId))
                           .Returns(() => request);
            
            var requestEncoder = this.Resolve<BadServerSaltHandler>();
            
            var channel = new EmbeddedChannel(requestEncoder);

            var badServerSalt = new TBadServerSalt
                                {
                                    BadMsgId = BadMsgId,
                                    NewServerSalt = Random.NextLong()
                                };

            // ---

            channel.WriteInbound(badServerSalt);

            await Task.Delay(500).ConfigureAwait(false);
            // ---
            
            Assert.Equal(request, channel.ReadOutbound<object>());
        }
    }
}