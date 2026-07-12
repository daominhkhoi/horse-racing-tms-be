	ALTER TABLE Horses ADD ImageUrl VARCHAR(255);
	ALTER TABLE Horses ADD Status NVARCHAR(50) DEFAULT 'Pending';
  -- Bước 1: Xóa khóa ngoại cũ (đang trỏ tới bảng Referee_Profiles)
	ALTER TABLE [Horse_Verifications]
	DROP CONSTRAINT [FK__Horse_Ver__Verif__5535A963];
	GO

	-- Bước 2: Tạo lại khóa ngoại mới (trỏ tới bảng Users)
	ALTER TABLE [Horse_Verifications]
	ADD CONSTRAINT [FK__Horse_Ver__Verif_Users] 
	FOREIGN KEY ([VerifiedBy]) REFERENCES [Users]([UserID]);
	GO