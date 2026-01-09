USE [master]
GO

/****** Object:  Database [ApplicationDbContext]    Script Date: 1/8/2026 11:41:33 PM ******/
CREATE DATABASE [ApplicationDbContext]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'ApplicationDbContext', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\ApplicationDbContext.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'ApplicationDbContext_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\ApplicationDbContext_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO

IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [ApplicationDbContext].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO

ALTER DATABASE [ApplicationDbContext] SET ANSI_NULL_DEFAULT OFF 
GO

ALTER DATABASE [ApplicationDbContext] SET ANSI_NULLS OFF 
GO

ALTER DATABASE [ApplicationDbContext] SET ANSI_PADDING OFF 
GO

ALTER DATABASE [ApplicationDbContext] SET ANSI_WARNINGS OFF 
GO

ALTER DATABASE [ApplicationDbContext] SET ARITHABORT OFF 
GO

ALTER DATABASE [ApplicationDbContext] SET AUTO_CLOSE OFF 
GO

ALTER DATABASE [ApplicationDbContext] SET AUTO_SHRINK OFF 
GO

ALTER DATABASE [ApplicationDbContext] SET AUTO_UPDATE_STATISTICS ON 
GO

ALTER DATABASE [ApplicationDbContext] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO

ALTER DATABASE [ApplicationDbContext] SET CURSOR_DEFAULT  GLOBAL 
GO

ALTER DATABASE [ApplicationDbContext] SET CONCAT_NULL_YIELDS_NULL OFF 
GO

ALTER DATABASE [ApplicationDbContext] SET NUMERIC_ROUNDABORT OFF 
GO

ALTER DATABASE [ApplicationDbContext] SET QUOTED_IDENTIFIER OFF 
GO

ALTER DATABASE [ApplicationDbContext] SET RECURSIVE_TRIGGERS OFF 
GO

ALTER DATABASE [ApplicationDbContext] SET  DISABLE_BROKER 
GO

ALTER DATABASE [ApplicationDbContext] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO

ALTER DATABASE [ApplicationDbContext] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO

ALTER DATABASE [ApplicationDbContext] SET TRUSTWORTHY OFF 
GO

ALTER DATABASE [ApplicationDbContext] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO

ALTER DATABASE [ApplicationDbContext] SET PARAMETERIZATION SIMPLE 
GO

ALTER DATABASE [ApplicationDbContext] SET READ_COMMITTED_SNAPSHOT OFF 
GO

ALTER DATABASE [ApplicationDbContext] SET HONOR_BROKER_PRIORITY OFF 
GO

ALTER DATABASE [ApplicationDbContext] SET RECOVERY SIMPLE 
GO

ALTER DATABASE [ApplicationDbContext] SET  MULTI_USER 
GO

ALTER DATABASE [ApplicationDbContext] SET PAGE_VERIFY CHECKSUM  
GO

ALTER DATABASE [ApplicationDbContext] SET DB_CHAINING OFF 
GO

ALTER DATABASE [ApplicationDbContext] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO

ALTER DATABASE [ApplicationDbContext] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO

ALTER DATABASE [ApplicationDbContext] SET DELAYED_DURABILITY = DISABLED 
GO

ALTER DATABASE [ApplicationDbContext] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO

ALTER DATABASE [ApplicationDbContext] SET QUERY_STORE = ON
GO

ALTER DATABASE [ApplicationDbContext] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO

ALTER DATABASE [ApplicationDbContext] SET  READ_WRITE 
GO


USE [ApplicationDbContext]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Shares]') AND type in (N'U'))
ALTER TABLE [dbo].[Shares] DROP CONSTRAINT IF EXISTS [FK_Shares_EntityTypes]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PendingPurge]') AND type in (N'U'))
ALTER TABLE [dbo].[PendingPurge] DROP CONSTRAINT IF EXISTS [FK_PendingPurge_EntityTypes]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Documents]') AND type in (N'U'))
ALTER TABLE [dbo].[Documents] DROP CONSTRAINT IF EXISTS [FK_Container_Documents]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Containers]') AND type in (N'U'))
ALTER TABLE [dbo].[Containers] DROP CONSTRAINT IF EXISTS [FK_Containers_Subscriptions]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Containers]') AND type in (N'U'))
ALTER TABLE [dbo].[Containers] DROP CONSTRAINT IF EXISTS [FK_Containers_Containers]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserTokens]') AND type in (N'U'))
ALTER TABLE [dbo].[AspNetUserTokens] DROP CONSTRAINT IF EXISTS [FK_AspNetUserTokens_AspNetUsers_UserId]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserRoles]') AND type in (N'U'))
ALTER TABLE [dbo].[AspNetUserRoles] DROP CONSTRAINT IF EXISTS [FK_AspNetUserRoles_AspNetUsers_UserId]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserRoles]') AND type in (N'U'))
ALTER TABLE [dbo].[AspNetUserRoles] DROP CONSTRAINT IF EXISTS [FK_AspNetUserRoles_AspNetRoles_RoleId]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserLogins]') AND type in (N'U'))
ALTER TABLE [dbo].[AspNetUserLogins] DROP CONSTRAINT IF EXISTS [FK_AspNetUserLogins_AspNetUsers_UserId]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserClaims]') AND type in (N'U'))
ALTER TABLE [dbo].[AspNetUserClaims] DROP CONSTRAINT IF EXISTS [FK_AspNetUserClaims_AspNetUsers_UserId]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetRoleClaims]') AND type in (N'U'))
ALTER TABLE [dbo].[AspNetRoleClaims] DROP CONSTRAINT IF EXISTS [FK_AspNetRoleClaims_AspNetRoles_RoleId]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Shares]') AND type in (N'U'))
ALTER TABLE [dbo].[Shares] DROP CONSTRAINT IF EXISTS [DF_Shares_Id]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Documents]') AND type in (N'U'))
ALTER TABLE [dbo].[Documents] DROP CONSTRAINT IF EXISTS [DF_Documents_ModifiedOn]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Documents]') AND type in (N'U'))
ALTER TABLE [dbo].[Documents] DROP CONSTRAINT IF EXISTS [DF_Documents_IsDeleted]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Containers]') AND type in (N'U'))
ALTER TABLE [dbo].[Containers] DROP CONSTRAINT IF EXISTS [DF_Containers_IsDeleted]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Containers]') AND type in (N'U'))
ALTER TABLE [dbo].[Containers] DROP CONSTRAINT IF EXISTS [DF_Folders_ModifiedOn]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Containers]') AND type in (N'U'))
ALTER TABLE [dbo].[Containers] DROP CONSTRAINT IF EXISTS [DF_Folders_Id]
GO
/****** Object:  Index [IX_Documents]    Script Date: 1/8/2026 11:37:24 PM ******/
DROP INDEX IF EXISTS [IX_Documents] ON [dbo].[Documents]
GO
/****** Object:  Index [UserNameIndex]    Script Date: 1/8/2026 11:37:24 PM ******/
DROP INDEX IF EXISTS [UserNameIndex] ON [dbo].[AspNetUsers]
GO
/****** Object:  Index [EmailIndex]    Script Date: 1/8/2026 11:37:24 PM ******/
DROP INDEX IF EXISTS [EmailIndex] ON [dbo].[AspNetUsers]
GO
/****** Object:  Index [IX_AspNetUserRoles_RoleId]    Script Date: 1/8/2026 11:37:24 PM ******/
DROP INDEX IF EXISTS [IX_AspNetUserRoles_RoleId] ON [dbo].[AspNetUserRoles]
GO
/****** Object:  Index [IX_AspNetUserLogins_UserId]    Script Date: 1/8/2026 11:37:24 PM ******/
DROP INDEX IF EXISTS [IX_AspNetUserLogins_UserId] ON [dbo].[AspNetUserLogins]
GO
/****** Object:  Index [IX_AspNetUserClaims_UserId]    Script Date: 1/8/2026 11:37:24 PM ******/
DROP INDEX IF EXISTS [IX_AspNetUserClaims_UserId] ON [dbo].[AspNetUserClaims]
GO
/****** Object:  Index [RoleNameIndex]    Script Date: 1/8/2026 11:37:24 PM ******/
DROP INDEX IF EXISTS [RoleNameIndex] ON [dbo].[AspNetRoles]
GO
/****** Object:  Index [IX_AspNetRoleClaims_RoleId]    Script Date: 1/8/2026 11:37:24 PM ******/
DROP INDEX IF EXISTS [IX_AspNetRoleClaims_RoleId] ON [dbo].[AspNetRoleClaims]
GO
/****** Object:  Table [dbo].[UserSubscriptions]    Script Date: 1/8/2026 11:37:24 PM ******/
DROP TABLE IF EXISTS [dbo].[UserSubscriptions]
GO
/****** Object:  Table [dbo].[Subscriptions]    Script Date: 1/8/2026 11:37:24 PM ******/
DROP TABLE IF EXISTS [dbo].[Subscriptions]
GO
/****** Object:  Table [dbo].[Shares]    Script Date: 1/8/2026 11:37:24 PM ******/
DROP TABLE IF EXISTS [dbo].[Shares]
GO
/****** Object:  Table [dbo].[PendingPurge]    Script Date: 1/8/2026 11:37:24 PM ******/
DROP TABLE IF EXISTS [dbo].[PendingPurge]
GO
/****** Object:  Table [dbo].[EntityTypes]    Script Date: 1/8/2026 11:37:24 PM ******/
DROP TABLE IF EXISTS [dbo].[EntityTypes]
GO
/****** Object:  Table [dbo].[Documents]    Script Date: 1/8/2026 11:37:24 PM ******/
DROP TABLE IF EXISTS [dbo].[Documents]
GO
/****** Object:  Table [dbo].[Containers]    Script Date: 1/8/2026 11:37:24 PM ******/
DROP TABLE IF EXISTS [dbo].[Containers]
GO
/****** Object:  Table [dbo].[AspNetUserTokens]    Script Date: 1/8/2026 11:37:24 PM ******/
DROP TABLE IF EXISTS [dbo].[AspNetUserTokens]
GO
/****** Object:  Table [dbo].[AspNetUsers]    Script Date: 1/8/2026 11:37:24 PM ******/
DROP TABLE IF EXISTS [dbo].[AspNetUsers]
GO
/****** Object:  Table [dbo].[AspNetUserRoles]    Script Date: 1/8/2026 11:37:24 PM ******/
DROP TABLE IF EXISTS [dbo].[AspNetUserRoles]
GO
/****** Object:  Table [dbo].[AspNetUserLogins]    Script Date: 1/8/2026 11:37:24 PM ******/
DROP TABLE IF EXISTS [dbo].[AspNetUserLogins]
GO
/****** Object:  Table [dbo].[AspNetUserClaims]    Script Date: 1/8/2026 11:37:24 PM ******/
DROP TABLE IF EXISTS [dbo].[AspNetUserClaims]
GO
/****** Object:  Table [dbo].[AspNetRoles]    Script Date: 1/8/2026 11:37:24 PM ******/
DROP TABLE IF EXISTS [dbo].[AspNetRoles]
GO
/****** Object:  Table [dbo].[AspNetRoleClaims]    Script Date: 1/8/2026 11:37:24 PM ******/
DROP TABLE IF EXISTS [dbo].[AspNetRoleClaims]
GO
/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 1/8/2026 11:37:24 PM ******/
DROP TABLE IF EXISTS [dbo].[__EFMigrationsHistory]
GO
USE [master]
GO
/****** Object:  Database [ApplicationDbContext]    Script Date: 1/8/2026 11:37:24 PM ******/
DROP DATABASE IF EXISTS [ApplicationDbContext]
GO
/****** Object:  Database [ApplicationDbContext]    Script Date: 1/8/2026 11:37:24 PM ******/
CREATE DATABASE [ApplicationDbContext]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'ApplicationDbContext', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\ApplicationDbContext.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'ApplicationDbContext_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\ApplicationDbContext_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [ApplicationDbContext] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [ApplicationDbContext].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [ApplicationDbContext] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [ApplicationDbContext] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [ApplicationDbContext] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [ApplicationDbContext] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [ApplicationDbContext] SET ARITHABORT OFF 
GO
ALTER DATABASE [ApplicationDbContext] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [ApplicationDbContext] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [ApplicationDbContext] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [ApplicationDbContext] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [ApplicationDbContext] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [ApplicationDbContext] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [ApplicationDbContext] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [ApplicationDbContext] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [ApplicationDbContext] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [ApplicationDbContext] SET  DISABLE_BROKER 
GO
ALTER DATABASE [ApplicationDbContext] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [ApplicationDbContext] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [ApplicationDbContext] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [ApplicationDbContext] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [ApplicationDbContext] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [ApplicationDbContext] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [ApplicationDbContext] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [ApplicationDbContext] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [ApplicationDbContext] SET  MULTI_USER 
GO
ALTER DATABASE [ApplicationDbContext] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [ApplicationDbContext] SET DB_CHAINING OFF 
GO
ALTER DATABASE [ApplicationDbContext] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [ApplicationDbContext] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [ApplicationDbContext] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [ApplicationDbContext] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [ApplicationDbContext] SET QUERY_STORE = ON
GO
ALTER DATABASE [ApplicationDbContext] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [ApplicationDbContext]
GO
/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 1/8/2026 11:37:24 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetRoleClaims]    Script Date: 1/8/2026 11:37:24 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoleClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RoleId] [nvarchar](450) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetRoles]    Script Date: 1/8/2026 11:37:24 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoles](
	[Id] [nvarchar](450) NOT NULL,
	[Name] [nvarchar](256) NULL,
	[NormalizedName] [nvarchar](256) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetRoles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserClaims]    Script Date: 1/8/2026 11:37:24 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](450) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserLogins]    Script Date: 1/8/2026 11:37:24 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserLogins](
	[LoginProvider] [nvarchar](450) NOT NULL,
	[ProviderKey] [nvarchar](450) NOT NULL,
	[ProviderDisplayName] [nvarchar](max) NULL,
	[UserId] [nvarchar](450) NOT NULL,
 CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY CLUSTERED 
(
	[LoginProvider] ASC,
	[ProviderKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserRoles]    Script Date: 1/8/2026 11:37:24 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserRoles](
	[UserId] [nvarchar](450) NOT NULL,
	[RoleId] [nvarchar](450) NOT NULL,
 CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUsers]    Script Date: 1/8/2026 11:37:24 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUsers](
	[Id] [nvarchar](450) NOT NULL,
	[UserName] [nvarchar](256) NULL,
	[NormalizedUserName] [nvarchar](256) NULL,
	[Email] [nvarchar](256) NULL,
	[NormalizedEmail] [nvarchar](256) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[SecurityStamp] [nvarchar](max) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
	[PhoneNumber] [nvarchar](max) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEnd] [datetimeoffset](7) NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
	[FirstName] [nvarchar](50) NULL,
	[LastName] [nvarchar](50) NULL,
 CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserTokens]    Script Date: 1/8/2026 11:37:24 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserTokens](
	[UserId] [nvarchar](450) NOT NULL,
	[LoginProvider] [nvarchar](450) NOT NULL,
	[Name] [nvarchar](450) NOT NULL,
	[Value] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[LoginProvider] ASC,
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Containers]    Script Date: 1/8/2026 11:37:24 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Containers](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
	[SubscriptionId] [uniqueidentifier] NOT NULL,
	[ParentId] [uniqueidentifier] NULL,
	[ModifiedOn] [datetime2](7) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[DeletedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_Folders] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UK_Folders] UNIQUE NONCLUSTERED 
(
	[Name] ASC,
	[SubscriptionId] ASC,
	[ParentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Documents]    Script Date: 1/8/2026 11:37:24 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Documents](
	[Id] [uniqueidentifier] NOT NULL,
	[ContainerId] [uniqueidentifier] NULL,
	[DeletedOn] [datetime2](7) NULL,
	[IsDeleted] [bit] NOT NULL,
	[Location] [nvarchar](2000) NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
	[ModifiedOn] [datetime2](7) NOT NULL,
	[SubscriptionId] [uniqueidentifier] NOT NULL,
	[Type] [nvarchar](256) NULL,
	[Extension] [nvarchar](100) NULL,
 CONSTRAINT [PK_Documents] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EntityTypes]    Script Date: 1/8/2026 11:37:24 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EntityTypes](
	[Id] [tinyint] NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
 CONSTRAINT [PK_ReferenceTypes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PendingPurge]    Script Date: 1/8/2026 11:37:24 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PendingPurge](
	[ReferenceId] [uniqueidentifier] NOT NULL,
	[SubscriptionId] [uniqueidentifier] NOT NULL,
	[EntityTypeId] [tinyint] NOT NULL,
 CONSTRAINT [PK_ReadyForPurge] PRIMARY KEY CLUSTERED 
(
	[ReferenceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Shares]    Script Date: 1/8/2026 11:37:24 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Shares](
	[Id] [uniqueidentifier] NOT NULL,
	[ReferenceId] [uniqueidentifier] NOT NULL,
	[EntityTypeId] [tinyint] NOT NULL,
	[Access] [nvarchar](max) NULL,
	[People] [nvarchar](max) NULL,
	[Owner] [nvarchar](250) NULL,
 CONSTRAINT [PK_Shares] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Subscriptions]    Script Date: 1/8/2026 11:37:24 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Subscriptions](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](1000) NULL,
 CONSTRAINT [PK_Table_1] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserSubscriptions]    Script Date: 1/8/2026 11:37:24 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserSubscriptions](
	[SubscriptionId] [uniqueidentifier] NOT NULL,
	[UserId] [nvarchar](450) NOT NULL
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AspNetRoleClaims_RoleId]    Script Date: 1/8/2026 11:37:24 PM ******/
CREATE NONCLUSTERED INDEX [IX_AspNetRoleClaims_RoleId] ON [dbo].[AspNetRoleClaims]
(
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [RoleNameIndex]    Script Date: 1/8/2026 11:37:24 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [RoleNameIndex] ON [dbo].[AspNetRoles]
(
	[NormalizedName] ASC
)
WHERE ([NormalizedName] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AspNetUserClaims_UserId]    Script Date: 1/8/2026 11:37:24 PM ******/
CREATE NONCLUSTERED INDEX [IX_AspNetUserClaims_UserId] ON [dbo].[AspNetUserClaims]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AspNetUserLogins_UserId]    Script Date: 1/8/2026 11:37:24 PM ******/
CREATE NONCLUSTERED INDEX [IX_AspNetUserLogins_UserId] ON [dbo].[AspNetUserLogins]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AspNetUserRoles_RoleId]    Script Date: 1/8/2026 11:37:24 PM ******/
CREATE NONCLUSTERED INDEX [IX_AspNetUserRoles_RoleId] ON [dbo].[AspNetUserRoles]
(
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [EmailIndex]    Script Date: 1/8/2026 11:37:24 PM ******/
CREATE NONCLUSTERED INDEX [EmailIndex] ON [dbo].[AspNetUsers]
(
	[NormalizedEmail] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UserNameIndex]    Script Date: 1/8/2026 11:37:24 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex] ON [dbo].[AspNetUsers]
(
	[NormalizedUserName] ASC
)
WHERE ([NormalizedUserName] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Documents]    Script Date: 1/8/2026 11:37:24 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Documents] ON [dbo].[Documents]
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Containers] ADD  CONSTRAINT [DF_Folders_Id]  DEFAULT (newsequentialid()) FOR [Id]
GO
ALTER TABLE [dbo].[Containers] ADD  CONSTRAINT [DF_Folders_ModifiedOn]  DEFAULT (getutcdate()) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[Containers] ADD  CONSTRAINT [DF_Containers_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Documents] ADD  CONSTRAINT [DF_Documents_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Documents] ADD  CONSTRAINT [DF_Documents_ModifiedOn]  DEFAULT (getutcdate()) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[Shares] ADD  CONSTRAINT [DF_Shares_Id]  DEFAULT (newsequentialid()) FOR [Id]
GO
ALTER TABLE [dbo].[AspNetRoleClaims]  WITH CHECK ADD  CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetRoleClaims] CHECK CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserClaims]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserClaims] CHECK CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserLogins]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserLogins] CHECK CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserTokens]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserTokens] CHECK CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[Containers]  WITH CHECK ADD  CONSTRAINT [FK_Containers_Containers] FOREIGN KEY([ParentId])
REFERENCES [dbo].[Containers] ([Id])
GO
ALTER TABLE [dbo].[Containers] CHECK CONSTRAINT [FK_Containers_Containers]
GO
ALTER TABLE [dbo].[Containers]  WITH CHECK ADD  CONSTRAINT [FK_Containers_Subscriptions] FOREIGN KEY([SubscriptionId])
REFERENCES [dbo].[Subscriptions] ([Id])
GO
ALTER TABLE [dbo].[Containers] CHECK CONSTRAINT [FK_Containers_Subscriptions]
GO
ALTER TABLE [dbo].[Documents]  WITH CHECK ADD  CONSTRAINT [FK_Container_Documents] FOREIGN KEY([ContainerId])
REFERENCES [dbo].[Containers] ([Id])
GO
ALTER TABLE [dbo].[Documents] CHECK CONSTRAINT [FK_Container_Documents]
GO
ALTER TABLE [dbo].[PendingPurge]  WITH CHECK ADD  CONSTRAINT [FK_PendingPurge_EntityTypes] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityTypes] ([Id])
GO
ALTER TABLE [dbo].[PendingPurge] CHECK CONSTRAINT [FK_PendingPurge_EntityTypes]
GO
ALTER TABLE [dbo].[Shares]  WITH CHECK ADD  CONSTRAINT [FK_Shares_EntityTypes] FOREIGN KEY([EntityTypeId])
REFERENCES [dbo].[EntityTypes] ([Id])
GO
ALTER TABLE [dbo].[Shares] CHECK CONSTRAINT [FK_Shares_EntityTypes]
GO
USE [master]
GO
ALTER DATABASE [ApplicationDbContext] SET  READ_WRITE 
GO
