# Documentation Branding Specification

## Requirements

### Requirement: Branding Asset Naming Convention

The repository MUST store packify-aligned branding assets under `app/assets/branding/` using the filenames `logo-main.png`, `logo-sidebar.png`, `icon-app.png`, and `banner-readme.png`.

#### Scenario: Branding files are discoverable by convention

- GIVEN a contributor needs branded assets
- WHEN the contributor opens `app/assets/branding/`
- THEN the standard filenames are present and unambiguous

### Requirement: README and Markdown Alignment

The README MUST use `banner-readme.png` as the primary banner asset, and project markdown documents SHOULD use the same product naming, install path, and update terminology as the README.

#### Scenario: README uses the standard banner

- GIVEN the root README is rendered
- WHEN the page header is shown
- THEN `app/assets/branding/banner-readme.png` is the referenced banner asset
