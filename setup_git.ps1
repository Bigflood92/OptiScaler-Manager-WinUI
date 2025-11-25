#!/bin/bash
# Automatic Git Setup Script for OptiScaler Manager WinUI
# This script will:
# 1. Initialize git repository
# 2. Create .gitignore
# 3. Stage all files
# 4. Create initial commit
# 5. Wait for you to create GitHub repo
# 6. Add remote and push

Write-Host "?? OptiScaler Manager - Git Setup Automation" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Gray
Write-Host ""

# Step 1: Check if git is initialized
Write-Host "?? Step 1: Checking Git status..." -ForegroundColor Yellow
$gitExists = Test-Path ".git"

if (-not $gitExists) {
    Write-Host "   Initializing Git repository..." -ForegroundColor White
    git init
    Write-Host "   ? Git initialized" -ForegroundColor Green
} else {
    Write-Host "   ? Git already initialized" -ForegroundColor Green
}
Write-Host ""

# Step 2: Check remote
Write-Host "?? Step 2: Checking Git remote..." -ForegroundColor Yellow
$remotes = git remote -v
if ($remotes) {
    Write-Host "   ??  Remote already exists:" -ForegroundColor Yellow
    Write-Host $remotes -ForegroundColor Gray
    Write-Host ""
    $continue = Read-Host "   Remove existing remote? (y/n)"
    if ($continue -eq "y") {
        git remote remove origin
        Write-Host "   ? Remote removed" -ForegroundColor Green
    }
} else {
    Write-Host "   ? No remote configured (ready for new repo)" -ForegroundColor Green
}
Write-Host ""

# Step 3: Stage all files
Write-Host "?? Step 3: Staging files for commit..." -ForegroundColor Yellow
Write-Host "   Adding all project files..." -ForegroundColor White

# Add specific folders first
git add OptiScaler.Core/
git add OptiScaler.UI/
git add docs/
git add *.md
git add *.sln
git add .gitignore

Write-Host "   ? Files staged" -ForegroundColor Green
Write-Host ""

# Step 4: Show what will be committed
Write-Host "?? Step 4: Files ready to commit:" -ForegroundColor Yellow
git status --short | Select-Object -First 20
Write-Host "   ... (and more)" -ForegroundColor Gray
Write-Host ""

# Step 5: Create commit
Write-Host "?? Step 5: Creating initial commit..." -ForegroundColor Yellow
$commitMsg = @"
Initial commit - OptiScaler Manager WinUI 3

? Features:
- WinUI 3 modern interface
- Game scanning (Steam/Epic/EA/Xbox)
- Mod installation system
- DLSS/FSR/XeSS support
- Global crash handler
- Review prompt service
- Store-ready assets
- Privacy policy

??? Architecture:
- .NET 8 with WinUI 3
- MVVM pattern
- Service-based design
- Self-contained deployment

?? Store Preparation:
- Package.appxmanifest configured
- All PNG assets included
- Privacy policy ready
- Documentation complete

Ready for Microsoft Store submission (Phase 1 complete)
"@

git commit -m $commitMsg
Write-Host "   ? Initial commit created" -ForegroundColor Green
Write-Host ""

# Step 6: Instructions for GitHub
Write-Host "================================================" -ForegroundColor Gray
Write-Host "?? NEXT: Create Private GitHub Repository" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Gray
Write-Host ""
Write-Host "1. Go to: https://github.com/new" -ForegroundColor White
Write-Host ""
Write-Host "2. Fill in:" -ForegroundColor White
Write-Host "   Name:        OptiScaler-Manager-WinUI" -ForegroundColor Yellow
Write-Host "   Description: Modern WinUI 3 version (.NET 8)" -ForegroundColor Gray
Write-Host "   Visibility:  ?? PRIVATE" -ForegroundColor Red
Write-Host "   Initialize:  ? NO (leave unchecked)" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Click 'Create repository'" -ForegroundColor White
Write-Host ""
Write-Host "4. Copy the HTTPS URL shown (will be like):" -ForegroundColor White
Write-Host "   https://github.com/Bigflood92/OptiScaler-Manager-WinUI.git" -ForegroundColor Cyan
Write-Host ""
Write-Host "================================================" -ForegroundColor Gray
Write-Host ""

$repoUrl = Read-Host "Paste the repository URL here (or press Enter to skip)"

if ($repoUrl) {
    Write-Host ""
    Write-Host "?? Step 7: Adding remote and pushing..." -ForegroundColor Yellow
    
    git remote add origin $repoUrl
    Write-Host "   ? Remote added: $repoUrl" -ForegroundColor Green
    
    Write-Host "   Pushing to GitHub..." -ForegroundColor White
    git push -u origin master
    
    Write-Host ""
    Write-Host "?? SUCCESS! Repository is now on GitHub (PRIVATE)" -ForegroundColor Green
    Write-Host ""
    Write-Host "?? Your private repo: $repoUrl" -ForegroundColor Cyan
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "??  Skipped remote setup" -ForegroundColor Yellow
    Write-Host "   You can add it later with:" -ForegroundColor Gray
    Write-Host "   git remote add origin <YOUR_REPO_URL>" -ForegroundColor Gray
    Write-Host "   git push -u origin master" -ForegroundColor Gray
    Write-Host ""
}

# Step 8: Setup GitHub Pages
Write-Host "================================================" -ForegroundColor Gray
Write-Host "?? NEXT: Enable GitHub Pages" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Gray
Write-Host ""
Write-Host "1. Go to your new repo on GitHub" -ForegroundColor White
Write-Host "2. Click 'Settings' tab" -ForegroundColor White
Write-Host "3. Scroll to 'Pages' in left sidebar" -ForegroundColor White
Write-Host "4. Under 'Source':" -ForegroundColor White
Write-Host "   Branch: master" -ForegroundColor Yellow
Write-Host "   Folder: /docs" -ForegroundColor Yellow
Write-Host "5. Click 'Save'" -ForegroundColor White
Write-Host "6. Wait 1-2 minutes" -ForegroundColor White
Write-Host ""
Write-Host "Your Privacy Policy will be at:" -ForegroundColor White
Write-Host "https://bigflood92.github.io/OptiScaler-Manager-WinUI/PrivacyPolicy.html" -ForegroundColor Cyan
Write-Host ""
Write-Host "================================================" -ForegroundColor Gray
Write-Host ""
Write-Host "? Git setup complete!" -ForegroundColor Green
Write-Host ""
