CREATE TABLE [dbo].[Users] (
    [UserId]       INT            IDENTITY (1, 1) NOT NULL,
    [Name]         NVARCHAR (100) NOT NULL,
    [Email]        NVARCHAR (100) NOT NULL,
    [RoleId]       INT            NOT NULL,
    [PasswordHash] NVARCHAR (255) NOT NULL,
    [IsActive]     BIT            DEFAULT ((1)) NOT NULL,
    [CreatedOn]    DATETIME       DEFAULT (getdate()) NOT NULL,
    [CreatedBy]    NVARCHAR (100) NULL,
    [ModifiedBy]   NVARCHAR (100) NULL,
    [ModifiedOn]   DATETIME       NULL,
    PRIMARY KEY CLUSTERED ([UserId] ASC),
    CONSTRAINT [FK_Users_UserRole] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[UserRole] ([RoleId])
);

