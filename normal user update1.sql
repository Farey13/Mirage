USE MirageDB;
GO

UPDATE Users
SET PasswordHash = '$2a$11$W2A.U.z5N23xV.n/iT.xTewhVp2g.Vz4O3E5B.w3w0jK6b8y5y7O6'
WHERE Username = 'normaluser';
GO