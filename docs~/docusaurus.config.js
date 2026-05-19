// @ts-check

const {themes} = require('prism-react-renderer');

const siteUrl = process.env.DOCS_URL || 'https://ustl-face-tracking.kamishiro.online';
const baseUrl = process.env.DOCS_BASE_URL || '/';

/** @type {import('@docusaurus/types').Config} */
const config = {
  title: 'U-Stella FaceTracking',
  tagline: 'Face tracking setup tool for VRChat avatars',
  url: siteUrl,
  baseUrl,
  favicon: 'img/favicon.ico',
  trailingSlash: true,
  onBrokenLinks: 'throw',
  markdown: {
    hooks: {
      onBrokenMarkdownLinks: 'warn',
    },
  },
  i18n: {
    defaultLocale: 'en',
    locales: ['en', 'ja'],
    localeConfigs: {
      en: {
        label: 'English',
        htmlLang: 'en-US',
      },
      ja: {
        label: '日本語',
        htmlLang: 'ja-JP',
      },
    },
  },
  presets: [
    [
      'classic',
      /** @type {import('@docusaurus/preset-classic').Options} */
      ({
        docs: {
          routeBasePath: '/',
          sidebarPath: require.resolve('./sidebars.js'),
        },
        blog: false,
        theme: {
          customCss: require.resolve('./src/css/custom.css'),
        },
      }),
    ],
  ],
  themeConfig:
    /** @type {import('@docusaurus/preset-classic').ThemeConfig} */
    ({
      navbar: {
        title: 'U-Stella FaceTracking',
        items: [
          {
            type: 'docSidebar',
            sidebarId: 'docs',
            position: 'left',
            label: 'Docs',
          },
          {
            type: 'localeDropdown',
            position: 'right',
          },
          {
            href: 'https://github.com/AoiKamishiro/ustl-face-tracking',
            label: 'GitHub',
            position: 'right',
          },
        ],
      },
      footer: {
        style: 'dark',
        links: [
          {
            title: 'Docs',
            items: [
              {
                label: 'Usage Guide',
                to: '/usage/',
              },
              {
                label: 'Adding Hardware Profiles',
                to: '/add-hardware-profile/',
              },
            ],
          },
          {
            title: 'Project',
            items: [
              {
                label: 'GitHub',
                href: 'https://github.com/AoiKamishiro/ustl-face-tracking',
              },
            ],
          },
        ],
        copyright: `Copyright © ${new Date().getFullYear()} U-Stella Inc. Built with Docusaurus.`,
      },
      prism: {
        theme: themes.github,
        darkTheme: themes.dracula,
      },
    }),
};

module.exports = config;
