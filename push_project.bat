@echo off
echo --- Git Push Script ---

:: STEP 1: Git identity
git config user.name "Arjanik Zakoyan"
git config user.email "arjanik.zakoyan@gmail.com"

:: STEP 2: Add files
git add .

:: STEP 3: Commit
git commit -m "Initial commit"

:: STEP 4: Create branch
git branch -M main

:: STEP 5: Set origin (միայն եթե չես արել)
git remote add origin https://github.com/arjanik-zakoyan/HotelStaffManagement.git 2>NUL

:: STEP 6: Push forcibly
git push -u origin main --force

echo --- DONE ---
pause