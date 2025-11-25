# ?? docs/ - GitHub Pages Documentation

This folder contains the **public-facing documentation** for OptiScaler Manager, hosted via GitHub Pages.

## ?? Live URLs (After GitHub Pages activation)

- **Landing Page:** https://bigflood92.github.io/OptiScaler-Manager/
- **Privacy Policy:** https://bigflood92.github.io/OptiScaler-Manager/PrivacyPolicy.html

---

## ?? Files in This Folder

| File | Purpose |
|------|---------|
| `index.html` | Landing page with app overview and download links |
| `PrivacyPolicy.html` | Privacy policy (required for Microsoft Store) |

---

## ?? Activation Instructions

### Step 1: Commit and Push

```bash
git add docs/
git commit -m "Add GitHub Pages documentation and privacy policy"
git push origin master
```

### Step 2: Enable GitHub Pages

1. Go to your GitHub repository: https://github.com/Bigflood92/OptiScaler-Manager
2. Click **Settings** (top tab)
3. Scroll to **Pages** (left sidebar)
4. Under **Source**:
   - Branch: `master` (or `main`)
   - Folder: `/docs`
5. Click **Save**
6. Wait 1-2 minutes for deployment

### Step 3: Verify

After deployment (GitHub will show "Your site is live"):

? Visit: https://bigflood92.github.io/OptiScaler-Manager/  
? Visit: https://bigflood92.github.io/OptiScaler-Manager/PrivacyPolicy.html

Both should load with the styled pages.

---

## ?? Update Package.appxmanifest

Once GitHub Pages is live, update `OptiScaler.UI/Package.appxmanifest`:

```xml
<Properties>
    <DisplayName>OptiScaler Manager</DisplayName>
    <PublisherDisplayName>Bigflood92</PublisherDisplayName>
    <Logo>Assets\StoreLogo.png</Logo>
    <Description>Modern Windows app for game optimization with FSR3 and DLSS mods.</Description>
    <uap:SupportUrl>https://github.com/Bigflood92/OptiScaler-Manager</uap:SupportUrl>
    
    <!-- ADD THIS LINE: -->
    <PrivacyPolicy>https://bigflood92.github.io/OptiScaler-Manager/PrivacyPolicy.html</PrivacyPolicy>
</Properties>
```

---

## ?? Updating Documentation

To update the privacy policy or landing page:

1. Edit files in `/docs` folder
2. Commit and push changes
3. GitHub Pages auto-deploys (1-2 minutes)
4. No manual action needed

---

## ? Checklist

- [ ] Commit /docs folder to git
- [ ] Push to GitHub
- [ ] Enable GitHub Pages in repo settings
- [ ] Wait for deployment (1-2 min)
- [ ] Test both URLs in browser
- [ ] Update Package.appxmanifest with Privacy Policy URL
- [ ] Rebuild project to include manifest changes

**Estimated time: 5-10 minutes**

---

## ?? Next Steps After This

Once Privacy Policy URL is working:
1. Optimize PNG assets (TinyPNG.com)
2. Capture screenshots
3. Complete keyboard/gamepad navigation
4. Add AutomationProperties
5. Submit to Microsoft Store!
