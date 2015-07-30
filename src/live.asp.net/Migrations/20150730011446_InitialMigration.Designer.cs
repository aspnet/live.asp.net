using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using live.asp.net.Data;

namespace live.asp.net.Migrations
{
    [ContextType(typeof(AppDbContext))]
    partial class InitialMigration
    {
        public override string Id
        {
            get { return "20150730011446_InitialMigration"; }
        }
        
        public override string ProductVersion
        {
            get { return "7.0.0-beta5-13518"; }
        }
        
        public override void BuildTargetModel(ModelBuilder builder)
        {
            builder
                .Annotation("SqlServer:DefaultSequenceName", "DefaultSequence")
                .Annotation("SqlServer:Sequence:.DefaultSequence", "'DefaultSequence', '', '1', '10', '', '', 'Int64', 'False'")
                .Annotation("SqlServer:ValueGeneration", "Sequence");
            
            builder.Entity("live.asp.net.Models.LiveShowDetails", b =>
                {
                    b.Property<int>("Id")
                        .GenerateValueOnAdd()
                        .StoreGeneratedPattern(StoreGeneratedPattern.Identity);
                    
                    b.Property<string>("AdminMessage");
                    
                    b.Property<string>("LiveShowEmbedUrl");
                    
                    b.Property<DateTime?>("NextShowDateUtc");
                    
                    b.Key("Id");
                });
        }
    }
}
