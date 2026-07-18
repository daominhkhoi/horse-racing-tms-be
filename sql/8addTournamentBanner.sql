ALTER TABLE Tournaments ADD BannerUrl NVARCHAR(MAX);
GO

-- Thêm data giả để test UI nếu cần
UPDATE Tournaments SET BannerUrl = 'https://images.unsplash.com/photo-1598285514030-802ec061c4d4?q=80&w=1200&auto=format&fit=crop' WHERE TourId = 4;
UPDATE Tournaments SET BannerUrl = 'https://images.unsplash.com/photo-1534068307254-8e527d425baf?q=80&w=1200&auto=format&fit=crop' WHERE TourId = 5;
UPDATE Tournaments SET BannerUrl = 'https://images.unsplash.com/photo-1498305886981-9b1d1985da38?q=80&w=1200&auto=format&fit=crop' WHERE BannerUrl IS NULL;
GO
