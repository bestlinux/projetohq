USE [projetohq]
GO

/****** Object:  Table [dbo].[HQs]    Script Date: 06/06/2021 16:27:12 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[HQs](
	[Id] [uniqueidentifier] NOT NULL,
	[Titulo] [nvarchar](max) NULL,
	[Editora] [nvarchar](max) NULL,
	[CreatedBy] [nvarchar](max) NULL,
	[Created] [datetime2](7) NOT NULL,
	[LastModifiedBy] [nvarchar](max) NULL,
	[LastModified] [datetime2](7) NULL,
	[AnoLancamento] [nvarchar](max) NULL,
	[NumeroEdicao] int NULL,
	[Genero] int NULL,
	[Status] int NULL,
	[Formato] int NULL,
	[LinkDetalhes] [nvarchar](max) NULL,
	[ThumbCapa] [nvarchar](max) NULL,
	[Categoria] int NULL,
 CONSTRAINT [PK_HQs] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


