-- Fix tournaments that are stuck in 'Upcoming' or 'Live' but all their races are 'Completed', 'Awarded' or 'Cancelled'
UPDATE Tournaments 
SET Status = 'Completed' 
WHERE TourId IN (
    SELECT TourId 
    FROM Races 
    GROUP BY TourId 
    HAVING COUNT(*) = SUM(CASE WHEN Status IN ('Completed', 'Awarded', 'Cancelled') THEN 1 ELSE 0 END) 
       AND SUM(CASE WHEN Status IN ('Completed', 'Awarded') THEN 1 ELSE 0 END) > 0
);

-- Fix tournaments that are stuck in 'Upcoming' but have at least one active race
UPDATE Tournaments 
SET Status = 'Live' 
WHERE Status = 'Upcoming' 
  AND TourId IN (
      SELECT TourId 
      FROM Races 
      WHERE Status IN ('Started', 'Live', 'Ongoing', 'Completed', 'Awarded')
  );
