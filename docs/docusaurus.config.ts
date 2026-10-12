import {themes as prismThemes} from 'prism-react-renderer';
import type {Config} from '@docusaurus/types';
import type * as Preset from '@docusaurus/preset-classic';

// This runs in Node.js - Don't use client-side code here (browser APIs, JSX...)

const config: Config = {
  title: 'PasteMyst Docs',
  tagline: 'Documentation for PasteMyst',
  favicon: 'img/favicon.ico',

  url: 'https://docs.paste.myst.rs',
  baseUrl: '/',

  trailingSlash: false,

  // Only used by `npm run deploy` (docusaurus deploy); CI deploys via .github/workflows/deploy-docs.yml.
  organizationName: 'pastemyst',
  projectName: 'pastemyst-v3',

  onBrokenLinks: 'throw',
  onBrokenMarkdownLinks: 'warn',

  i18n: {
    defaultLocale: 'en',
    locales: ['en'],
  },

  presets: [
    [
      'classic',
      {
        docs: {
          sidebarPath: './sidebars.ts',
          editUrl:
            'https://github.com/pastemyst/pastemyst-v3/tree/main/docs',
          routeBasePath: '/'
        },
        blog: {
          blogTitle: 'Blog',
          blogDescription: 'News and release notes for PasteMyst, the open source pastebin.',
          showReadingTime: true,
          feedOptions: {
            type: ['rss', 'atom'],
            title: 'PasteMyst blog',
            description: 'News and release notes for PasteMyst, the open source pastebin.',
            xslt: true,
          },
          editUrl:
            'https://github.com/pastemyst/pastemyst-v3/tree/main/docs',
          onInlineTags: 'warn',
          onInlineAuthors: 'warn',
          onUntruncatedBlogPosts: 'warn',
        },
        theme: {
          customCss: './src/css/custom.css',
        },
        sitemap: {
          // Listing pages with nothing of their own
          ignorePatterns: ['/blog/archive', '/blog/authors', '/blog/authors/**', '/blog/tags', '/blog/tags/**', '/blog/page/**'],
        },
      } satisfies Preset.Options,
    ],
  ],

  themeConfig: {
    colorMode: {
      defaultMode: 'dark',
      respectPrefersColorScheme: true
    },
    image: 'img/social-card.png',
    navbar: {
      title: 'PasteMyst Docs',
      logo: {
        alt: 'pastemyst logo',
        src: 'img/pastemyst.svg',
      },
      items: [
        {
          type: 'docSidebar',
          sidebarId: 'tutorialSidebar',
          position: 'left',
          label: 'API Docs',
        },
        {to: '/cli', label: 'CLI Tool', position: 'left'},
        {to: '/blog', label: 'Blog', position: 'left'},
        {
          href: 'https://github.com/pastemyst/pastemyst-v3',
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
              label: 'API Docs',
              to: '/',
            },
          ],
        },
        {
          title: 'Community',
          items: [
            {
              label: 'Discord',
              href: 'https://discord.gg/SdKbcbq',
            },
          ],
        },
        {
          title: 'More',
          items: [
            {
              label: 'Blog',
              to: '/blog',
            },
            {
              label: 'GitHub',
              href: 'https://github.com/pastemyst/pastemyst-v3',
            },
          ],
        },
      ],
      copyright: `Copyright © ${new Date().getFullYear()} CodeMyst. Built with Docusaurus.`,
    },
    prism: {
      theme: prismThemes.gruvboxMaterialLight,
      darkTheme: prismThemes.gruvboxMaterialDark,
      additionalLanguages: ['http']
    },
  } satisfies Preset.ThemeConfig,
};

export default config;
