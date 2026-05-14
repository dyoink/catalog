--
-- PostgreSQL database dump
--

\restrict ICyHUNnxm4ZwxKxkGz0AItkN4PpiNrEISeynLiNB8g4f6skXWOHDCGCY8YBcPuo

-- Dumped from database version 16.13
-- Dumped by pg_dump version 16.13

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: pg_trgm; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS pg_trgm WITH SCHEMA public;


--
-- Name: EXTENSION pg_trgm; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION pg_trgm IS 'text similarity measurement and index searching based on trigrams';


--
-- Name: unaccent; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS unaccent WITH SCHEMA public;


--
-- Name: EXTENSION unaccent; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION unaccent IS 'text search dictionary that removes accents';


--
-- Name: uuid-ossp; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS "uuid-ossp" WITH SCHEMA public;


--
-- Name: EXTENSION "uuid-ossp"; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION "uuid-ossp" IS 'generate universally unique identifiers (UUIDs)';


--
-- Name: product_status; Type: TYPE; Schema: public; Owner: aquacms
--

CREATE TYPE public.product_status AS ENUM (
    'available',
    'out_of_stock',
    'hidden'
);


ALTER TYPE public.product_status OWNER TO aquacms;

--
-- Name: user_role; Type: TYPE; Schema: public; Owner: aquacms
--

CREATE TYPE public.user_role AS ENUM (
    'SUPER_ADMIN',
    'MANAGER',
    'EDITOR',
    'SALE'
);


ALTER TYPE public.user_role OWNER TO aquacms;

--
-- Name: CAST (text AS public.product_status); Type: CAST; Schema: -; Owner: -
--

CREATE CAST (text AS public.product_status) WITH INOUT AS IMPLICIT;


--
-- Name: CAST (text AS public.user_role); Type: CAST; Schema: -; Owner: -
--

CREATE CAST (text AS public.user_role) WITH INOUT AS IMPLICIT;


--
-- Name: fn_update_timestamp(); Type: FUNCTION; Schema: public; Owner: aquacms
--

CREATE FUNCTION public.fn_update_timestamp() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$;


ALTER FUNCTION public.fn_update_timestamp() OWNER TO aquacms;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: activity_logs; Type: TABLE; Schema: public; Owner: aquacms
--

CREATE TABLE public.activity_logs (
    id uuid DEFAULT public.uuid_generate_v4() NOT NULL,
    user_id uuid,
    user_name character varying(100) NOT NULL,
    action character varying(50) NOT NULL,
    entity_type character varying(50) NOT NULL,
    entity_id character varying(100),
    description text,
    ip_address character varying(45),
    user_agent character varying(500),
    severity character varying(20) DEFAULT 'Info'::character varying NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.activity_logs OWNER TO aquacms;

--
-- Name: banners; Type: TABLE; Schema: public; Owner: aquacms
--

CREATE TABLE public.banners (
    id uuid DEFAULT public.uuid_generate_v4() NOT NULL,
    title character varying(255) NOT NULL,
    subtitle character varying(255),
    description text,
    image character varying(500) NOT NULL,
    color character varying(100),
    link_url character varying(500),
    sort_order integer DEFAULT 0 NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.banners OWNER TO aquacms;

--
-- Name: categories; Type: TABLE; Schema: public; Owner: aquacms
--

CREATE TABLE public.categories (
    id uuid DEFAULT public.uuid_generate_v4() NOT NULL,
    name character varying(100) NOT NULL,
    slug character varying(120) NOT NULL,
    image character varying(500),
    sort_order integer DEFAULT 0 NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.categories OWNER TO aquacms;

--
-- Name: chat_messages; Type: TABLE; Schema: public; Owner: aquacms
--

CREATE TABLE public.chat_messages (
    id uuid DEFAULT public.uuid_generate_v4() NOT NULL,
    session_id uuid NOT NULL,
    sender_id character varying(50) NOT NULL,
    is_from_admin boolean DEFAULT false NOT NULL,
    text text NOT NULL,
    is_read boolean DEFAULT false NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.chat_messages OWNER TO aquacms;

--
-- Name: chat_sessions; Type: TABLE; Schema: public; Owner: aquacms
--

CREATE TABLE public.chat_sessions (
    id uuid DEFAULT public.uuid_generate_v4() NOT NULL,
    guest_id character varying(50) NOT NULL,
    unread_count integer DEFAULT 0 NOT NULL,
    last_message text,
    last_seen_at timestamp with time zone,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.chat_sessions OWNER TO aquacms;

--
-- Name: knowledge_categories; Type: TABLE; Schema: public; Owner: aquacms
--

CREATE TABLE public.knowledge_categories (
    id uuid DEFAULT public.uuid_generate_v4() NOT NULL,
    name character varying(100) NOT NULL,
    slug character varying(120) NOT NULL,
    sort_order integer DEFAULT 0 NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.knowledge_categories OWNER TO aquacms;

--
-- Name: page_views; Type: TABLE; Schema: public; Owner: aquacms
--

CREATE TABLE public.page_views (
    id bigint NOT NULL,
    path character varying(500) NOT NULL,
    entity_id uuid,
    entity_type character varying(20),
    ip_address character varying(45),
    user_agent character varying(500),
    referrer character varying(500),
    viewed_at date DEFAULT CURRENT_DATE NOT NULL
);


ALTER TABLE public.page_views OWNER TO aquacms;

--
-- Name: page_views_id_seq; Type: SEQUENCE; Schema: public; Owner: aquacms
--

CREATE SEQUENCE public.page_views_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.page_views_id_seq OWNER TO aquacms;

--
-- Name: page_views_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: aquacms
--

ALTER SEQUENCE public.page_views_id_seq OWNED BY public.page_views.id;


--
-- Name: partner_categories; Type: TABLE; Schema: public; Owner: aquacms
--

CREATE TABLE public.partner_categories (
    id uuid DEFAULT public.uuid_generate_v4() NOT NULL,
    name character varying(100) NOT NULL,
    slug character varying(120) NOT NULL,
    sort_order integer DEFAULT 0 NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.partner_categories OWNER TO aquacms;

--
-- Name: partners; Type: TABLE; Schema: public; Owner: aquacms
--

CREATE TABLE public.partners (
    id uuid DEFAULT public.uuid_generate_v4() NOT NULL,
    short_id bigint NOT NULL,
    slug character varying(200) NOT NULL,
    name character varying(255) NOT NULL,
    description text,
    detailed_description text,
    partner_category_id uuid,
    location character varying(255),
    since character varying(10),
    image character varying(500),
    contact_email character varying(255),
    contact_phone character varying(30),
    website character varying(500),
    is_active boolean DEFAULT true NOT NULL,
    sort_order integer DEFAULT 0 NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.partners OWNER TO aquacms;

--
-- Name: partners_short_id_seq; Type: SEQUENCE; Schema: public; Owner: aquacms
--

CREATE SEQUENCE public.partners_short_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.partners_short_id_seq OWNER TO aquacms;

--
-- Name: partners_short_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: aquacms
--

ALTER SEQUENCE public.partners_short_id_seq OWNED BY public.partners.short_id;


--
-- Name: posts; Type: TABLE; Schema: public; Owner: aquacms
--

CREATE TABLE public.posts (
    id uuid DEFAULT public.uuid_generate_v4() NOT NULL,
    short_id bigint NOT NULL,
    slug character varying(200) NOT NULL,
    title character varying(255) NOT NULL,
    excerpt text,
    content text NOT NULL,
    image character varying(500),
    author character varying(100) DEFAULT 'Admin'::character varying NOT NULL,
    knowledge_category_id uuid,
    read_time character varying(20),
    is_published boolean DEFAULT false NOT NULL,
    published_at timestamp with time zone,
    view_count integer DEFAULT 0 NOT NULL,
    meta_title character varying(70),
    meta_desc character varying(160),
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.posts OWNER TO aquacms;

--
-- Name: posts_short_id_seq; Type: SEQUENCE; Schema: public; Owner: aquacms
--

CREATE SEQUENCE public.posts_short_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.posts_short_id_seq OWNER TO aquacms;

--
-- Name: posts_short_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: aquacms
--

ALTER SEQUENCE public.posts_short_id_seq OWNED BY public.posts.short_id;


--
-- Name: product_contents; Type: TABLE; Schema: public; Owner: aquacms
--

CREATE TABLE public.product_contents (
    product_id uuid NOT NULL,
    description text,
    content_blocks jsonb DEFAULT '[]'::jsonb NOT NULL,
    image character varying(500),
    video_url character varying(500)
);


ALTER TABLE public.product_contents OWNER TO aquacms;

--
-- Name: product_finances; Type: TABLE; Schema: public; Owner: aquacms
--

CREATE TABLE public.product_finances (
    product_id uuid NOT NULL,
    price numeric(18,0),
    show_price boolean DEFAULT true NOT NULL,
    is_featured boolean DEFAULT false NOT NULL
);


ALTER TABLE public.product_finances OWNER TO aquacms;

--
-- Name: product_metadata; Type: TABLE; Schema: public; Owner: aquacms
--

CREATE TABLE public.product_metadata (
    product_id uuid NOT NULL,
    slug character varying(200) NOT NULL,
    meta_title character varying(70),
    meta_desc character varying(160)
);


ALTER TABLE public.product_metadata OWNER TO aquacms;

--
-- Name: product_statistics; Type: TABLE; Schema: public; Owner: aquacms
--

CREATE TABLE public.product_statistics (
    product_id uuid NOT NULL,
    view_count integer DEFAULT 0 NOT NULL
);


ALTER TABLE public.product_statistics OWNER TO aquacms;

--
-- Name: products; Type: TABLE; Schema: public; Owner: aquacms
--

CREATE TABLE public.products (
    id uuid DEFAULT public.uuid_generate_v4() NOT NULL,
    short_id bigint NOT NULL,
    sku character varying(100),
    name character varying(255) NOT NULL,
    category_id uuid,
    status character varying(20) DEFAULT 'hidden'::character varying NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.products OWNER TO aquacms;

--
-- Name: products_short_id_seq; Type: SEQUENCE; Schema: public; Owner: aquacms
--

CREATE SEQUENCE public.products_short_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.products_short_id_seq OWNER TO aquacms;

--
-- Name: products_short_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: aquacms
--

ALTER SEQUENCE public.products_short_id_seq OWNED BY public.products.short_id;


--
-- Name: site_settings; Type: TABLE; Schema: public; Owner: aquacms
--

CREATE TABLE public.site_settings (
    id uuid DEFAULT public.uuid_generate_v4() NOT NULL,
    company_name character varying(255) DEFAULT 'AquaCMS'::character varying NOT NULL,
    logo character varying(500),
    address text,
    phone character varying(30),
    email character varying(255),
    facebook character varying(500),
    show_facebook boolean DEFAULT true NOT NULL,
    zalo character varying(500),
    show_zalo boolean DEFAULT true NOT NULL,
    youtube character varying(500),
    show_youtube boolean DEFAULT false NOT NULL,
    tiktok character varying(500),
    show_tiktok boolean DEFAULT false NOT NULL,
    telegram character varying(500),
    show_telegram boolean DEFAULT false NOT NULL,
    show_hotline boolean DEFAULT true NOT NULL,
    bank_name character varying(255),
    bank_number character varying(50),
    bank_owner character varying(255),
    background_color character varying(20) DEFAULT '#F9F9F9'::character varying,
    primary_color character varying(20) DEFAULT '#55B3D9'::character varying,
    navbar_color character varying(20) DEFAULT '#2563eb'::character varying,
    footer_color character varying(20) DEFAULT '#1F2937'::character varying,
    footer_text character varying(500),
    show_footer boolean DEFAULT true NOT NULL,
    show_banners boolean DEFAULT true NOT NULL,
    show_categories boolean DEFAULT true NOT NULL,
    show_featured_products boolean DEFAULT true NOT NULL,
    show_latest_posts boolean DEFAULT true NOT NULL,
    show_partners boolean DEFAULT true NOT NULL,
    featured_products_count integer DEFAULT 8 NOT NULL,
    latest_posts_count integer DEFAULT 6 NOT NULL,
    show_nav_products boolean DEFAULT true NOT NULL,
    show_nav_knowledge boolean DEFAULT true NOT NULL,
    show_nav_partners boolean DEFAULT true NOT NULL,
    show_nav_cart boolean DEFAULT true NOT NULL,
    about_title character varying(255),
    about_content text,
    about_image character varying(500),
    default_meta_title character varying(70),
    default_meta_description character varying(160),
    default_og_image character varying(500),
    google_analytics_id character varying(50),
    facebook_pixel_id character varying(50),
    footer_about_text text,
    copyright_text character varying(255),
    email_enabled boolean DEFAULT false NOT NULL,
    smtp_host character varying(255),
    smtp_port integer DEFAULT 587 NOT NULL,
    smtp_use_ssl boolean DEFAULT true NOT NULL,
    smtp_user character varying(255),
    smtp_password character varying(255),
    smtp_from_email character varying(255),
    smtp_from_name character varying(255),
    notification_email character varying(255),
    chat_auto_reply_message text,
    updated_at timestamp with time zone DEFAULT now() NOT NULL,
    show_nav_about boolean DEFAULT true NOT NULL
);


ALTER TABLE public.site_settings OWNER TO aquacms;

--
-- Name: users; Type: TABLE; Schema: public; Owner: aquacms
--

CREATE TABLE public.users (
    id uuid DEFAULT public.uuid_generate_v4() NOT NULL,
    name character varying(100) NOT NULL,
    email character varying(255) NOT NULL,
    password_hash character varying(255) NOT NULL,
    role public.user_role DEFAULT 'EDITOR'::public.user_role NOT NULL,
    avatar character varying(500),
    is_active boolean DEFAULT true NOT NULL,
    last_login_at timestamp with time zone,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.users OWNER TO aquacms;

--
-- Name: page_views id; Type: DEFAULT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.page_views ALTER COLUMN id SET DEFAULT nextval('public.page_views_id_seq'::regclass);


--
-- Name: partners short_id; Type: DEFAULT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.partners ALTER COLUMN short_id SET DEFAULT nextval('public.partners_short_id_seq'::regclass);


--
-- Name: posts short_id; Type: DEFAULT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.posts ALTER COLUMN short_id SET DEFAULT nextval('public.posts_short_id_seq'::regclass);


--
-- Name: products short_id; Type: DEFAULT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.products ALTER COLUMN short_id SET DEFAULT nextval('public.products_short_id_seq'::regclass);


--
-- Data for Name: activity_logs; Type: TABLE DATA; Schema: public; Owner: aquacms
--

COPY public.activity_logs (id, user_id, user_name, action, entity_type, entity_id, description, ip_address, user_agent, severity, created_at) FROM stdin;
b93fc54d-fd3c-49c0-97e4-ae4d5f2f049a	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	UPDATE	Product	76f9323c-7c5c-4535-beaf-2539d6ee7c74	Máy cho ăn mini 180°	127.0.0.1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	Success	2026-05-05 03:51:02.539024+00
828eeee3-3e6e-4df0-98c7-4c2b9d6a1444	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	UPDATE	Product	65a75d11-eb4a-4579-a970-370eb2e7e62c	Máy cho ăn thông minh IoT	127.0.0.1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	Success	2026-05-05 03:51:18.095485+00
f985145a-aeec-4cf3-8326-40da6e234705	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	EXPORT	Product	\N	Xuất Excel 3 sản phẩm	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	Success	2026-05-07 04:12:58.212601+00
147c4172-1a84-4088-b038-5f3dd8fff7f7	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	DELETE	KnowledgeCategory	f8ad96d2-97ca-40aa-8bcf-1c2dbf5526e0	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	Warning	2026-05-07 04:19:13.001101+00
2028d9c8-acb7-43fa-8e37-b5114c592059	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	DELETE	PartnerCategory	2de84afe-8f4e-44b5-84de-5af6139f974b	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	Warning	2026-05-07 04:19:27.052436+00
39804223-9847-4f7b-a8ad-a16e58ee6284	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	CREATE	Product	880b988a-91f0-426c-b670-f7622180a61a	1	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	Success	2026-05-07 04:20:21.185613+00
16357545-0c0f-4b17-b045-23b3dcd5d0af	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	DELETE	Product	880b988a-91f0-426c-b670-f7622180a61a	1	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	Warning	2026-05-07 04:20:43.559409+00
a26d7df5-0f0b-4d70-8a12-2dfd8867d9cd	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	CREATE	KnowledgeCategory	0e2ec0d8-7b3f-4c17-9945-92d48135414b	1	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	Success	2026-05-07 04:53:20.739535+00
a59f17f8-39f4-4f20-a30f-7d1a1cb87d0f	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	DELETE	KnowledgeCategory	0e2ec0d8-7b3f-4c17-9945-92d48135414b	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	Warning	2026-05-07 04:53:28.967836+00
c9c2664e-b4dd-4331-9926-99e32db266d1	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	CREATE	Product	c3bd10f7-522b-4804-8dae-086ad58c3724	1	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	Success	2026-05-07 04:54:37.276946+00
d58155fa-c462-4e12-8155-d615693369f0	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	DELETE	Product	c3bd10f7-522b-4804-8dae-086ad58c3724	1	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	Warning	2026-05-07 04:55:37.780501+00
cdc595fb-e61b-4914-9cfb-8dee81909036	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	CREATE	Product	3dbe43c6-efda-47ce-af8c-de4c41934ce4	Máy trộn cám FANTO 1B\\50L	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	Success	2026-05-07 05:15:44.511589+00
e354d745-69e1-491f-9839-118d853f4657	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	UPDATE	Product	3dbe43c6-efda-47ce-af8c-de4c41934ce4	Máy trộn cám FANTO 1B\\50L	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	Success	2026-05-07 05:21:30.116375+00
5f9c2eb4-62f8-4af2-b59f-ec951529fd7c	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	UPDATE	Product	3dbe43c6-efda-47ce-af8c-de4c41934ce4	Máy trộn cám FANTO 1B\\50L	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	Success	2026-05-07 05:21:47.764348+00
6512b428-ffb8-43ef-a693-3ab040f66e41	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	CREATE	Product	b7e4b5dd-c224-4850-aa29-acd6ac6d9583	Máy trộn cám FANTO 2B\\75L	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	Success	2026-05-07 05:27:29.633561+00
d145c632-6f4f-48d1-9671-e6d14491e58d	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	CREATE	Product	bd6721df-0e0a-48db-92ec-7c43c5d07a8f	Máy trộn cám FANTO 3B\\100L	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	Success	2026-05-07 05:29:38.536738+00
3982912a-435e-45e2-818e-0df2aaa0e8e1	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	UPDATE	Product	b7e4b5dd-c224-4850-aa29-acd6ac6d9583	Máy trộn cám FANTO 2B\\75L	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	Success	2026-05-07 05:29:54.016425+00
03b40a32-8576-482e-9f14-92aff2eb5b5c	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	CREATE	Product	a32266ef-95dc-456c-b3b8-592a6bd7fd4f	Máy trộn cám FANTO 4B\\150L	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	Success	2026-05-07 05:32:05.998867+00
fb45efcb-7949-454d-abd2-d7a3f69a78c6	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	CREATE	Product	6ad5fa9a-afdf-44e7-8d94-7355dc7207f5	Máy cho cá ăn 2 bao nhựa FANTO	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	Success	2026-05-07 05:41:20.392119+00
c47c5aea-932e-4233-805f-62865b1bf185	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	CREATE	Product	6d84884f-7db5-4167-85f1-4d8d9b0a97cd	Máy cho cá ăn 3 bao nhựa FANTO	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	Success	2026-05-07 05:45:27.320556+00
684bcce4-1a31-41c9-a6b0-002bc16bf65f	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	EXPORT	Product	\N	Xuất Excel 6 sản phẩm	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	Success	2026-05-07 06:29:44.529689+00
3b458a43-05a4-47ea-8b5f-e86ad2c300ff	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	EXPORT	Product	\N	Xuất Excel 6 sản phẩm	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	Success	2026-05-07 06:35:45.240202+00
dedd7970-b8fc-4231-a7ba-63da9571723e	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	UPDATE	Product	7eefff53-8079-4ec3-a756-b49ca1157a74	Quạt biến tần Tam Hoa FANTO 2.200W	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	Success	2026-05-07 06:48:19.345125+00
cad32fe4-09d0-4f6a-af82-09dd1bf03529	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	UPDATE	Product	e6489dd6-926a-4f4b-9976-08e6252822b4	Quạt biến tần Tam Hoa FANTO 1.500W	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	Success	2026-05-07 06:48:35.70153+00
2ebfa50d-2741-4905-99a3-acf1df732c7b	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	UPDATE	Product	8fe19fa8-e4a7-4317-861c-cad29f35168c	Quạt biến tần Tam Hoa FANTO 1.100W	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	Success	2026-05-07 06:48:48.894575+00
8e8241eb-c8c9-4ff7-8e29-874c89716966	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	CHANGE_PASSWORD	User	2c3e30b2-950a-4939-abe5-6a11aa1331d3	Đổi mật khẩu	127.0.0.1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	Success	2026-05-07 09:12:22.106546+00
dc38ae81-92b0-4c8a-8131-98092596ddf6	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	UPDATE	Product	3dbe43c6-efda-47ce-af8c-de4c41934ce4	Máy trộn cám FANTO 1B/50L	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	Success	2026-05-07 09:30:01.732081+00
d70ad198-1a40-4c58-9254-cc1abb6a7334	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	UPDATE	Product	b7e4b5dd-c224-4850-aa29-acd6ac6d9583	Máy trộn cám FANTO 2B/75L	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	Success	2026-05-07 09:30:24.915315+00
a361d8c7-711a-4953-bbbd-aac348e1c951	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	UPDATE	Product	bd6721df-0e0a-48db-92ec-7c43c5d07a8f	Máy trộn cám FANTO 3B/100L	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	Success	2026-05-07 09:30:37.791979+00
dd17194a-e2de-41d4-a06b-192eb11d95d9	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	UPDATE	Product	a32266ef-95dc-456c-b3b8-592a6bd7fd4f	Máy trộn cám FANTO 4B/150L	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	Success	2026-05-07 09:30:48.56689+00
9869fc18-c73a-4b05-848d-bb1d8b071073	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	UPDATE	Product	7eefff53-8079-4ec3-a756-b49ca1157a74	Quạt biến tần Tam Hoa FANTO 2.200W	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	Success	2026-05-07 09:52:09.319612+00
2dfd2fe1-d4b0-43e4-a2e0-a9faa0c08be9	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	UPDATE	Product	e6489dd6-926a-4f4b-9976-08e6252822b4	Quạt biến tần Tam Hoa FANTO 1.500W	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	Success	2026-05-07 09:52:17.0796+00
23f4a03d-63f7-4ddd-b901-41eff27ccdc5	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	UPDATE	Product	8fe19fa8-e4a7-4317-861c-cad29f35168c	Quạt biến tần Tam Hoa FANTO 1.100W	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	Success	2026-05-07 09:52:25.151028+00
36e5b5ac-3f73-47b8-ae58-ec8cc8dab8d3	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	CREATE	Product	a894f615-157f-4c75-b11c-6bbf3c103d31	sản phẩm test	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	Success	2026-05-12 07:59:46.175466+00
da51588e-ae38-498c-8a44-431072a19e5a	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	DELETE	Product	a894f615-157f-4c75-b11c-6bbf3c103d31	sản phẩm test	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	Warning	2026-05-12 08:00:11.383688+00
95d57a2f-b1c1-49c3-8a0b-07231a45d909	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	CREATE	Banner	7e047a5f-9e99-4892-b882-58ff07c65b94	banner 1	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	Success	2026-05-12 08:02:13.71159+00
54aab340-b4ca-42ce-95cc-e9c1af1a3028	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	DELETE	Banner	7e047a5f-9e99-4892-b882-58ff07c65b94	banner 1	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	Warning	2026-05-12 08:04:20.043016+00
c1ebc541-a80c-4680-8117-8102b2092287	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	CREATE	Product	1c4b06e4-2814-47a8-8646-c177ec6581c4	1	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	Success	2026-05-12 09:55:35.888581+00
7c1dd44d-490b-49ef-ad3f-4ef74b57cbbd	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	DELETE	Product	1c4b06e4-2814-47a8-8646-c177ec6581c4	1	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	Warning	2026-05-12 09:56:04.838112+00
8e4456c8-a265-4844-8cc7-ddc3241338de	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	CREATE	Product	4c99da55-0197-4e9d-a2ac-ce6cf61d8928	sản phẩm test	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	Success	2026-05-12 10:02:58.328969+00
b1db8ae3-6a71-44de-9341-ce0ba39b003f	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	DELETE	Product	4c99da55-0197-4e9d-a2ac-ce6cf61d8928	sản phẩm test	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	Warning	2026-05-12 10:03:33.507948+00
2dbdbc72-149b-4975-99fc-7c400222079c	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	CREATE	Banner	8eab2cee-e2ef-4388-96bb-6f5e6b62f715	Khuyến mãi mùa hè	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	Success	2026-05-12 10:04:28.037221+00
35fb6820-e8a5-4408-83fd-7d7abfc8d1e4	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	UPDATE	Banner	8eab2cee-e2ef-4388-96bb-6f5e6b62f715	Khuyến mãi mùa hè	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	Success	2026-05-12 10:04:54.715508+00
c08b2098-3542-4d78-8b15-8b5407cd79ef	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	UPDATE	SiteSettings	b57a2a7f-4b8e-4702-9f0e-4273965c8cdc	Cập nhật cài đặt site	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	Success	2026-05-12 10:06:23.320399+00
768bd5a9-2f27-4ab1-ae68-026feaa38c81	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	DELETE	Banner	8eab2cee-e2ef-4388-96bb-6f5e6b62f715	Khuyến mãi mùa hè	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	Warning	2026-05-12 10:07:55.772755+00
5cea6f4a-9d97-4f06-871a-a598454890c7	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	UPDATE	SiteSettings	b57a2a7f-4b8e-4702-9f0e-4273965c8cdc	Cập nhật cài đặt site	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	Success	2026-05-13 09:34:44.972707+00
efe5f7f1-4c86-4484-85bc-97eee810b960	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	UPDATE	SiteSettings	b57a2a7f-4b8e-4702-9f0e-4273965c8cdc	Cập nhật cài đặt site	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	Success	2026-05-13 09:35:28.795739+00
101df81b-9f5b-4797-8636-e23d30457f57	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	UPDATE	SiteSettings	b57a2a7f-4b8e-4702-9f0e-4273965c8cdc	Cập nhật cài đặt site	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	Success	2026-05-13 11:51:55.417248+00
334819b8-f377-4efe-ac9c-8456014e70e5	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	UPDATE	SiteSettings	b57a2a7f-4b8e-4702-9f0e-4273965c8cdc	Cập nhật cài đặt site	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	Success	2026-05-14 06:43:10.271096+00
72007f8b-b1a0-40f9-9092-a3dcce1ef8f8	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	UPDATE	SiteSettings	b57a2a7f-4b8e-4702-9f0e-4273965c8cdc	Cập nhật cài đặt site	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	Success	2026-05-14 06:43:34.027682+00
ac6bb6ae-5c37-4e6a-ae08-71e1965c36b8	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	UPDATE	SiteSettings	b57a2a7f-4b8e-4702-9f0e-4273965c8cdc	Cập nhật cài đặt site	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	Success	2026-05-14 06:54:04.975175+00
e8d14438-11a6-40f5-9503-187d2ecb9484	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	UPDATE	SiteSettings	b57a2a7f-4b8e-4702-9f0e-4273965c8cdc	Cập nhật cài đặt site	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	Success	2026-05-14 07:03:24.190404+00
9df4c5e1-8389-4050-8010-d6773cae2258	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	UPDATE	SiteSettings	b57a2a7f-4b8e-4702-9f0e-4273965c8cdc	Cập nhật cài đặt site	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	Success	2026-05-14 07:30:29.179197+00
908c9dee-38cb-4b5e-9392-ed731002882a	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	UPDATE	SiteSettings	b57a2a7f-4b8e-4702-9f0e-4273965c8cdc	Cập nhật cài đặt site	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	Success	2026-05-14 08:33:28.698829+00
4cbd9a28-2029-472b-a913-d9dfa7771977	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	CREATE	Banner	21b6e179-04fc-4c2c-a112-cda67d47ff43	1	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	Success	2026-05-14 09:15:13.892141+00
9360e4ee-4a24-44f7-9598-6706f390a28e	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	UPDATE	SiteSettings	b57a2a7f-4b8e-4702-9f0e-4273965c8cdc	Cập nhật cài đặt site	127.0.0.1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	Success	2026-05-14 09:40:04.809007+00
bbdc62be-1297-4095-beae-3d8072966273	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	UPDATE	SiteSettings	b57a2a7f-4b8e-4702-9f0e-4273965c8cdc	Cập nhật cài đặt site	127.0.0.1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	Success	2026-05-14 09:40:13.810886+00
d4cf8ac2-432f-40d4-8e18-e421018fbe94	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	UPDATE	SiteSettings	b57a2a7f-4b8e-4702-9f0e-4273965c8cdc	Cập nhật cài đặt site	127.0.0.1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	Success	2026-05-14 09:40:21.525791+00
19ef7c22-e544-4b40-aa69-53f1bd941a61	2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	UPDATE	SiteSettings	b57a2a7f-4b8e-4702-9f0e-4273965c8cdc	Cập nhật cài đặt site	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	Success	2026-05-14 09:43:25.82215+00
\.


--
-- Data for Name: banners; Type: TABLE DATA; Schema: public; Owner: aquacms
--

COPY public.banners (id, title, subtitle, description, image, color, link_url, sort_order, is_active, created_at) FROM stdin;
7e23adb0-6043-4dee-9e0a-53d35395aaea	Máy cho tôm ăn thế hệ mới	SẢN PHẨM MỚI 2025	Công nghệ xoay 360°, IoT tích hợp, điều khiển qua smartphone. Tiết kiệm 30% thức ăn.	https://placehold.co/800x500/55B3D9/FFF?text=M%C3%A1y+cho+%C4%83n+360%C2%B0	\N	/san-pham	1	t	2026-05-05 02:45:38.133573+00
21b6e179-04fc-4c2c-a112-cda67d47ff43	1	1	1	/uploads/banners/67f34fbb474a40cc917dbf8aff09e10a.jpg	\N	\N	1	t	2026-05-14 09:15:13.860789+00
\.


--
-- Data for Name: categories; Type: TABLE DATA; Schema: public; Owner: aquacms
--

COPY public.categories (id, name, slug, image, sort_order, created_at) FROM stdin;
f6ff294a-eb36-47b3-82e3-acf886b20e0f	Máy Trộn Cám	may-tron-cam	\N	0	2026-05-07 04:43:57.951652+00
531a00d4-ad41-42a5-89b6-b3ddf6016c8e	Máy Cho Cá Ăn	may-cho-ca-an	\N	1	2026-05-07 04:44:15.538928+00
d010f78f-b1a6-4244-9590-b6b34703c894	Quạt Nước Tạo Dòng	quat-nuoc-tao-dong	\N	2	2026-05-07 04:44:47.134805+00
7aa74630-97fa-4783-8827-2f7e68297707	Siêu Oxy FANTO	sieu-oxy-fanto	\N	3	2026-05-07 04:51:24.80811+00
22419e79-f027-4cbc-a59a-74808a6e740b	Tam Hoa Biến Tần FANTO	tam-hoa-bien-tan-fanto	\N	4	2026-05-07 04:51:36.85485+00
a8a8c4e1-e3a2-4e11-96ed-b02c9cfd068e	Tạo Sóng 1 Tác Dụng	tao-song-1-tac-dung	\N	5	2026-05-07 04:51:46.905535+00
3143e24c-2fb8-4899-9460-8e975d8722c7	Tạo Sóng 2 Tác Dụng	tao-song-2-tac-dung	\N	6	2026-05-07 04:52:03.47317+00
a8e339cc-2954-42c6-ba32-5a2353383dfe	Máy Thổi Khí	may-thoi-khi	\N	7	2026-05-07 04:52:11.92912+00
4ec8697d-aef1-4210-bd51-b9c40e3fcaa0	Bơm Chìm Inox	bom-chim-inox	\N	8	2026-05-07 04:52:22.541386+00
9676fe3c-1c74-4281-a7c2-2f40282ca3c7	Bơm Mưa	bom-mua	\N	9	2026-05-07 04:55:05.248406+00
\.


--
-- Data for Name: chat_messages; Type: TABLE DATA; Schema: public; Owner: aquacms
--

COPY public.chat_messages (id, session_id, sender_id, is_from_admin, text, is_read, created_at) FROM stdin;
9c809d7b-b34d-4f43-800d-ec6570354f82	9eaeb236-0374-4adc-afa9-3e377cbe98fc	g_byol4v2dcmos1xuwj	f	e	t	2026-05-07 06:53:44.321863+00
6268bcdf-dde9-42b7-826a-ce7fb2be454f	9eaeb236-0374-4adc-afa9-3e377cbe98fc	admin	t	s	f	2026-05-07 06:53:55.11088+00
52cfba23-b0ab-42fd-95f8-9d73d619f2d1	9eaeb236-0374-4adc-afa9-3e377cbe98fc	g_byol4v2dcmos1xuwj	f	s	t	2026-05-07 06:53:58.145828+00
4ca002d2-bb70-4283-ba2f-fbef59ac6bd9	9eaeb236-0374-4adc-afa9-3e377cbe98fc	g_byol4v2dcmos1xuwj	f	s	t	2026-05-07 07:16:21.649665+00
83534780-b586-4986-9c2b-f2b1593be04c	9eaeb236-0374-4adc-afa9-3e377cbe98fc	admin	t	a	t	2026-05-07 07:16:29.69737+00
8b3df43e-549c-4350-9fd0-e23775644f05	9eaeb236-0374-4adc-afa9-3e377cbe98fc	g_byol4v2dcmos1xuwj	f	a	t	2026-05-07 09:06:38.669929+00
5a5836b1-1b61-4fe6-8687-d74c2693bf08	9eaeb236-0374-4adc-afa9-3e377cbe98fc	admin	t	a	t	2026-05-07 09:06:52.48118+00
7961f1c5-ea11-470c-869b-25aa28f7640b	9eaeb236-0374-4adc-afa9-3e377cbe98fc	g_byol4v2dcmos1xuwj	f	a	t	2026-05-07 09:06:55.939675+00
aa74572f-d2da-4a18-aade-aebd88f5f9d1	9eaeb236-0374-4adc-afa9-3e377cbe98fc	g_byol4v2dcmos1xuwj	f	s	t	2026-05-07 09:07:05.232718+00
a103ff1c-2c69-4994-a1b9-d8caf05b344d	9eaeb236-0374-4adc-afa9-3e377cbe98fc	g_byol4v2dcmos1xuwj	f	s	t	2026-05-07 09:07:06.558554+00
d10e4537-4b47-49fe-8007-5d5266414779	9eaeb236-0374-4adc-afa9-3e377cbe98fc	g_byol4v2dcmos1xuwj	f	s	t	2026-05-07 09:07:07.684195+00
f3d1bf3d-75f4-478d-9671-6714f368eec3	9eaeb236-0374-4adc-afa9-3e377cbe98fc	g_byol4v2dcmos1xuwj	f	helo	t	2026-05-07 09:11:44.571855+00
7b227beb-3902-48b8-af40-eadcce89d83f	9eaeb236-0374-4adc-afa9-3e377cbe98fc	g_byol4v2dcmos1xuwj	f	hello	t	2026-05-12 10:05:11.198054+00
3f9606a9-1a93-4f72-a5be-d39dd0d9a9c0	9eaeb236-0374-4adc-afa9-3e377cbe98fc	admin	t	hello 1111	t	2026-05-12 10:05:22.333122+00
adf275f5-aef7-4736-b5fe-768c5d489241	9eaeb236-0374-4adc-afa9-3e377cbe98fc	g_byol4v2dcmos1xuwj	f	he	t	2026-05-12 10:05:26.384797+00
fd2b82c6-085b-436b-bb20-2c41bfc14695	9eaeb236-0374-4adc-afa9-3e377cbe98fc	g_byol4v2dcmos1xuwj	f	h	t	2026-05-12 10:06:29.839746+00
d7670f7f-b5fb-4388-a48b-728c37e90756	9eaeb236-0374-4adc-afa9-3e377cbe98fc	g_byol4v2dcmos1xuwj	f	hello	t	2026-05-12 10:06:46.368011+00
872be98d-a343-4172-a9f5-3176257f2db6	983bcb0c-9765-40f9-9d20-ede006a12a3a	admin	t	xin chào	t	2026-05-12 10:07:36.950813+00
40bc6788-f69a-4a6f-b012-c893803c171b	983bcb0c-9765-40f9-9d20-ede006a12a3a	g_78p2n9qb1mp2gvnrf	f	hello	t	2026-05-12 10:07:35.427036+00
\.


--
-- Data for Name: chat_sessions; Type: TABLE DATA; Schema: public; Owner: aquacms
--

COPY public.chat_sessions (id, guest_id, unread_count, last_message, last_seen_at, created_at, updated_at) FROM stdin;
9eaeb236-0374-4adc-afa9-3e377cbe98fc	g_byol4v2dcmos1xuwj	0	hello	\N	2026-05-07 06:53:44.303793+00	2026-05-12 10:06:58.804881+00
983bcb0c-9765-40f9-9d20-ede006a12a3a	g_78p2n9qb1mp2gvnrf	0	xin chào	\N	2026-05-12 10:07:35.426507+00	2026-05-13 10:20:38.037043+00
\.


--
-- Data for Name: knowledge_categories; Type: TABLE DATA; Schema: public; Owner: aquacms
--

COPY public.knowledge_categories (id, name, slug, sort_order, created_at) FROM stdin;
75074d2b-d594-4102-bc32-51be9c68284b	Hướng dẫn	huong-dan	1	2026-05-05 02:45:04.289758+00
aaa447a1-ca47-4450-9a1c-63b5133d420a	Kỹ thuật	ky-thuat	2	2026-05-05 02:45:04.289758+00
9b520564-b4c4-4111-bcc3-14e3d0f041f9	Quản lý ao nuôi	quan-ly-ao-nuoi	2	2026-05-05 02:45:37.99503+00
90ea5976-d6f0-4406-b11b-cbffe7e0bd28	Kỹ thuật nuôi tôm	ky-thuat-nuoi-tom	1	2026-05-05 02:45:37.994921+00
b0d51244-8901-4bfc-a2a3-ef7210a87a52	Tin tức ngành	tin-tuc-nganh	3	2026-05-05 02:45:37.99503+00
\.


--
-- Data for Name: page_views; Type: TABLE DATA; Schema: public; Owner: aquacms
--

COPY public.page_views (id, path, entity_id, entity_type, ip_address, user_agent, referrer, viewed_at) FROM stdin;
1	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36 Edg/147.0.0.0	http://localhost:5088/admin/banners	2026-05-05
2	/san-pham/may-cho-tom-an-tu-dong-360-3	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36 Edg/147.0.0.0	http://localhost:5088/	2026-05-05
3	/san-pham	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36 Edg/147.0.0.0	http://localhost:5088/san-pham/may-cho-tom-an-tu-dong-360-3	2026-05-05
4	/kien-thuc	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36 Edg/147.0.0.0	http://localhost:5088/san-pham	2026-05-05
5	/doi-tac	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36 Edg/147.0.0.0	http://localhost:5088/kien-thuc	2026-05-05
6	/san-pham/may-cho-an-mini-180-2	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36 Edg/147.0.0.0	http://localhost:5088/san-pham	2026-05-05
7	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/admin/settings	2026-05-05
8	/danh-muc/hoa-chat-xu-ly	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-05
9	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/admin/settings	2026-05-05
10	/danh-muc/hoa-chat-xu-ly	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-05
11	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/admin/settings	2026-05-05
12	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/admin/settings	2026-05-05
13	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/admin/settings	2026-05-05
14	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/admin/settings	2026-05-05
15	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/admin/settings	2026-05-05
16	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/admin/settings	2026-05-05
17	/danh-muc/hoa-chat-xu-ly	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-05
18	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/admin/settings	2026-05-05
19	/	\N	\N	127.0.0.1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/admin/settings	2026-05-05
20	/danh-muc/phu-kien-ao-nuoi	\N	\N	127.0.0.1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-05
21	/danh-muc/may-suc-khi	\N	\N	127.0.0.1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-05
22	/san-pham	\N	\N	127.0.0.1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-05
23	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-05
24	/san-pham/may-cho-an-thong-minh-iot-1	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-05
25	/san-pham	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-05
26	/kien-thuc	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/san-pham	2026-05-05
27	/kien-thuc/5-buoc-chuan-bi-ao-nuoi-tom-dat-chuan-1	\N	post	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/kien-thuc	2026-05-05
28	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/kien-thuc	2026-05-05
29	/danh-muc/may-cho-tom-an	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-05
30	/san-pham	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-05
31	/kien-thuc	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/san-pham	2026-05-05
32	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/kien-thuc	2026-05-05
33	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/san-pham	2026-05-05
34	/san-pham	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-05
35	/kien-thuc	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/san-pham	2026-05-05
36	/doi-tac	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/kien-thuc	2026-05-05
37	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/doi-tac	2026-05-05
38	/san-pham	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-05
39	/kien-thuc	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/san-pham	2026-05-05
40	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/admin/categories	2026-05-05
41	/san-pham	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-05
42	/doi-tac	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-05
43	/kien-thuc	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/doi-tac	2026-05-05
44	/gio-hang	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/doi-tac	2026-05-05
45	/san-pham/may-cho-an-mini-180-2	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/san-pham	2026-05-05
46	/kien-thuc/5-buoc-chuan-bi-ao-nuoi-tom-dat-chuan-1	\N	post	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/kien-thuc	2026-05-05
47	/doi-tac/cong-ty-thuy-san-mien-tay-1	\N	partner	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/doi-tac	2026-05-05
48	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/admin/products	2026-05-07
49	/san-pham/may-tron-cam-fanto-1b-50l-9	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-07
50	/san-pham/may-tron-cam-fanto-1b-50l-9	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-07
51	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/san-pham/may-tron-cam-fanto-1b-50l-9	2026-05-07
52	/danh-muc/may-tron-cam	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-07
53	/san-pham/may-tron-cam-fanto-2b-75l-10	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-07
54	/san-pham/may-tron-cam-fanto-3b-100l-11	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-07
55	/san-pham/may-cho-ca-an-2-bao-nhua-fanto-13	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-07
56	/san-pham/may-cho-ca-an-2-bao-nhua-fanto-13	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-07
57	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/san-pham/may-cho-ca-an-2-bao-nhua-fanto-13	2026-05-07
58	/san-pham/may-sieu-oxy-fanto-10-canh-2200w-380v-40	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-07
59	/san-pham	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-07
60	/san-pham/bom-mua-fanto-sc15-220v-22	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/san-pham	2026-05-07
61	/danh-muc/may-thoi-khi	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/san-pham	2026-05-07
62	/san-pham/may-thoi-khi-duoi-nuoc-fanto-hre65l-16	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/danh-muc/may-thoi-khi	2026-05-07
63	/san-pham/may-thoi-khi-duoi-nuoc-fanto-hre50l-48	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/san-pham/may-thoi-khi-duoi-nuoc-fanto-hre65l-16	2026-05-07
64	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/san-pham/may-thoi-khi-duoi-nuoc-fanto-hre50l-48	2026-05-07
65	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/admin	2026-05-07
66	/san-pham/may-tao-song-1-tac-dung-fanto-sc075-380v-41	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-07
67	/gio-hang	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/san-pham/may-tao-song-1-tac-dung-fanto-sc075-380v-41	2026-05-07
68	/kien-thuc	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/gio-hang	2026-05-07
69	/kien-thuc/5-buoc-chuan-bi-ao-nuoi-tom-dat-chuan-1	\N	post	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/kien-thuc	2026-05-07
70	/san-pham	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-07
71	/danh-muc/may-cho-ca-an	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/san-pham	2026-05-07
72	/danh-muc/quat-nuoc-tao-dong	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/danh-muc/may-cho-ca-an	2026-05-07
73	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/kien-thuc	2026-05-07
74	/san-pham/may-tron-cam-fanto-4b-150l-12	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-07
75	/san-pham/may-tron-cam-fanto-2b-75l-10	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-07
76	/san-pham/may-cho-ca-an-3-bao-inox-fanto-21	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-07
77	/san-pham/may-cho-ca-an-12-bao-inox-360-2-voi-fanto-24	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-07
78	/san-pham/quat-nuoc-tao-dong-fanto-4-canh-1hp-220v-23	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/147.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-07
79	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	\N	2026-05-12
80	/danh-muc/sieu-oxy-fanto	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-12
81	/san-pham/may-cho-ca-an-6-bao-inox-xoay-360-fanto-45	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-12
82	/gio-hang	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/san-pham/may-cho-ca-an-6-bao-inox-xoay-360-fanto-45	2026-05-12
83	/san-pham	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/gio-hang	2026-05-12
84	/danh-muc/may-cho-ca-an	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/san-pham	2026-05-12
85	/danh-muc/quat-nuoc-tao-dong	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/danh-muc/may-cho-ca-an	2026-05-12
86	/danh-muc/tam-hoa-bien-tan-fanto	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/danh-muc/sieu-oxy-fanto	2026-05-12
87	/danh-muc/tao-song-1-tac-dung	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/danh-muc/tam-hoa-bien-tan-fanto	2026-05-12
88	/danh-muc/tao-song-2-tac-dung	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/danh-muc/tao-song-1-tac-dung	2026-05-12
89	/danh-muc/may-thoi-khi	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/danh-muc/tao-song-2-tac-dung	2026-05-12
90	/danh-muc/bom-chim-inox	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/danh-muc/may-thoi-khi	2026-05-12
91	/danh-muc/bom-mua	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/danh-muc/bom-chim-inox	2026-05-12
92	/kien-thuc	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/danh-muc/bom-mua	2026-05-12
93	/kien-thuc/5-buoc-chuan-bi-ao-nuoi-tom-dat-chuan-1	\N	post	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/kien-thuc	2026-05-12
94	/doi-tac	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/kien-thuc/5-buoc-chuan-bi-ao-nuoi-tom-dat-chuan-1	2026-05-12
95	/doi-tac/cong-ty-thuy-san-mien-tay-1	\N	partner	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/doi-tac	2026-05-12
96	/san-pham/may-cho-ca-an-2-bao-nhua-fanto-13	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/doi-tac/cong-ty-thuy-san-mien-tay-1	2026-05-12
97	/san-pham/san-pham-test-49	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-12
98	/san-pham/may-tron-cam-fanto-1b-50l-9	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-12
99	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/san-pham/may-tron-cam-fanto-1b-50l-9	2026-05-12
100	/gio-hang	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-12
101	/danh-muc/quat-nuoc-tao-dong	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-12
102	/san-pham	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/danh-muc/quat-nuoc-tao-dong	2026-05-12
103	/danh-muc/may-tron-cam	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/san-pham	2026-05-12
104	/danh-muc/may-cho-ca-an	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/danh-muc/may-tron-cam	2026-05-12
105	/danh-muc/sieu-oxy-fanto	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/danh-muc/quat-nuoc-tao-dong	2026-05-12
106	/danh-muc/tam-hoa-bien-tan-fanto	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/danh-muc/sieu-oxy-fanto	2026-05-12
107	/danh-muc/tao-song-1-tac-dung	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/danh-muc/tam-hoa-bien-tan-fanto	2026-05-12
108	/san-pham/may-cho-ca-an-3-bao-nhua-fanto-14	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-12
109	/san-pham/may-cho-ca-an-3-bao-inox-fanto-21	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/san-pham/may-cho-ca-an-3-bao-nhua-fanto-14	2026-05-12
110	/kien-thuc	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/san-pham/may-cho-ca-an-3-bao-inox-fanto-21	2026-05-12
111	/kien-thuc/5-buoc-chuan-bi-ao-nuoi-tom-dat-chuan-1	\N	post	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/kien-thuc	2026-05-12
112	/doi-tac	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/kien-thuc	2026-05-12
113	/doi-tac/cong-ty-thuy-san-mien-tay-1	\N	partner	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/doi-tac	2026-05-12
114	/san-pham/may-tao-song-2-tac-dung-fanto-sc15-380v-19	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/doi-tac/cong-ty-thuy-san-mien-tay-1	2026-05-12
115	/danh-muc/may-tron-cam	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-12
116	/san-pham/may-tron-cam-fanto-3b-100l-11	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/danh-muc/may-tron-cam	2026-05-12
117	/san-pham/may-tron-cam-fanto-2b-75l-10	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/san-pham/may-tron-cam-fanto-3b-100l-11	2026-05-12
118	/kien-thuc	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/san-pham/may-tron-cam-fanto-2b-75l-10	2026-05-12
119	/kien-thuc/5-buoc-chuan-bi-ao-nuoi-tom-dat-chuan-1	\N	post	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/kien-thuc	2026-05-12
120	/doi-tac	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/kien-thuc/5-buoc-chuan-bi-ao-nuoi-tom-dat-chuan-1	2026-05-12
121	/doi-tac/cong-ty-thuy-san-mien-tay-1	\N	partner	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/doi-tac	2026-05-12
122	/gio-hang	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/doi-tac/cong-ty-thuy-san-mien-tay-1	2026-05-12
123	/san-pham	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/gio-hang	2026-05-12
124	/san-pham/bom-chim-inox-fanto-4kw-17	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/san-pham	2026-05-12
125	/san-pham/may-cho-ca-an-2-bao-nhua-fanto-13	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/gio-hang	2026-05-12
126	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/gio-hang	2026-05-12
127	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/danh-muc/may-tron-cam	2026-05-12
128	/san-pham/1-50	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-12
129	/danh-muc/sieu-oxy-fanto	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-12
130	/danh-muc/bom-mua	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/danh-muc/sieu-oxy-fanto	2026-05-12
131	/danh-muc/tao-song-2-tac-dung	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/danh-muc/bom-mua	2026-05-12
132	/san-pham/may-tao-song-2-tac-dung-fanto-sc15-380v-19	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/danh-muc/tao-song-2-tac-dung	2026-05-12
133	/san-pham/may-tao-song-2-tac-dung-fanto-sc15-220v-28	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/san-pham/may-tao-song-2-tac-dung-fanto-sc15-380v-19	2026-05-12
134	/gio-hang	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/san-pham/may-tao-song-2-tac-dung-fanto-sc15-220v-28	2026-05-12
135	/kien-thuc	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/san-pham/may-tao-song-2-tac-dung-fanto-sc15-220v-28	2026-05-12
136	/kien-thuc/5-buoc-chuan-bi-ao-nuoi-tom-dat-chuan-1	\N	post	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/kien-thuc	2026-05-12
137	/doi-tac	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/kien-thuc/5-buoc-chuan-bi-ao-nuoi-tom-dat-chuan-1	2026-05-12
138	/doi-tac/cong-ty-thuy-san-mien-tay-1	\N	partner	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/doi-tac	2026-05-12
139	/san-pham	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/admin	2026-05-12
140	/san-pham/may-tron-cam-fanto-1b-50l-9	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/admin	2026-05-12
141	/san-pham/san-pham-test-51	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/	2026-05-12
142	/san-pham/bom-mua-fanto-sc15-220v-22	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36	http://localhost:5088/san-pham	2026-05-12
143	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/dang-nhap?ReturnUrl=%2Fadmin	2026-05-13
144	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/	2026-05-13
145	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/dang-nhap?ReturnUrl=%2Fadmin	2026-05-13
146	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/	2026-05-13
147	/san-pham/may-sieu-oxy-fanto-6-canh-1500w-380v-44	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/	2026-05-13
148	/san-pham	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/san-pham/may-sieu-oxy-fanto-6-canh-1500w-380v-44	2026-05-13
149	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/san-pham	2026-05-13
150	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/san-pham	2026-05-13
151	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/admin/settings	2026-05-14
152	/san-pham/bom-chim-inox-fanto-4kw-17	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/	2026-05-14
153	/san-pham/bom-chim-inox-fanto-4kw-17	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/	2026-05-14
154	/san-pham/bom-chim-inox-fanto-4kw-17	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/	2026-05-14
155	/san-pham/bom-chim-inox-fanto-4kw-17	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/	2026-05-14
156	/san-pham/bom-chim-inox-fanto-4kw-17	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/	2026-05-14
157	/san-pham/bom-chim-inox-fanto-4kw-17	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/	2026-05-14
158	/san-pham/bom-chim-inox-fanto-4kw-17	\N	product	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/	2026-05-14
159	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/san-pham/bom-chim-inox-fanto-4kw-17	2026-05-14
160	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/san-pham/bom-chim-inox-fanto-4kw-17	2026-05-14
161	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/san-pham/bom-chim-inox-fanto-4kw-17	2026-05-14
162	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/san-pham/bom-chim-inox-fanto-4kw-17	2026-05-14
163	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/	2026-05-14
164	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/	2026-05-14
165	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/	2026-05-14
166	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/	2026-05-14
167	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/	2026-05-14
168	/danh-muc/bom-mua	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/	2026-05-14
169	/	\N	\N	127.0.0.1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/	2026-05-14
170	/san-pham	\N	\N	127.0.0.1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/	2026-05-14
171	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/san-pham	2026-05-14
172	/Home/About	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/	2026-05-14
173	/Home/About	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/	2026-05-14
174	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/Home/About	2026-05-14
175	/	\N	\N	127.0.0.1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/Home/About	2026-05-14
176	/san-pham	\N	\N	127.0.0.1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/	2026-05-14
177	/kien-thuc	\N	\N	127.0.0.1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/san-pham	2026-05-14
178	/doi-tac	\N	\N	127.0.0.1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/kien-thuc	2026-05-14
179	/doi-tac	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/kien-thuc	2026-05-14
180	/doi-tac/cong-ty-thuy-san-mien-tay-1	\N	partner	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/doi-tac	2026-05-14
181	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/doi-tac	2026-05-14
182	/san-pham	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/	2026-05-14
183	/kien-thuc	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/san-pham	2026-05-14
184	/	\N	\N	127.0.0.1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/	2026-05-14
185	/Home/About	\N	\N	127.0.0.1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/	2026-05-14
186	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/Home/About	2026-05-14
187	/san-pham	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/	2026-05-14
188	/kien-thuc	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/san-pham	2026-05-14
189	/doi-tac	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/kien-thuc	2026-05-14
190	/Home/About	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/doi-tac	2026-05-14
191	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/khong-co-quyen?ReturnUrl=%2Fadmin%2Fabout	2026-05-14
192	/Home/About	\N	\N	127.0.0.1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/doi-tac	2026-05-14
193	/	\N	\N	127.0.0.1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/Home/About	2026-05-14
194	/Home/About	\N	\N	127.0.0.1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/	2026-05-14
195	/Home/About	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/	2026-05-14
196	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/Home/About	2026-05-14
197	/	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/Home/About	2026-05-14
198	/Home/About	\N	\N	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36 Edg/148.0.0.0	http://localhost:5088/	2026-05-14
\.


--
-- Data for Name: partner_categories; Type: TABLE DATA; Schema: public; Owner: aquacms
--

COPY public.partner_categories (id, name, slug, sort_order, created_at) FROM stdin;
b33e0e4f-00f9-4032-a989-336d3d37d29e	Doanh nghiệp	doanh-nghiep	0	2026-05-05 02:45:04.292671+00
1b3b30af-c46b-405f-960e-9dab1e49fd7a	Nhà phân phối	nha-phan-phoi	1	2026-05-05 02:45:38.067658+00
\.


--
-- Data for Name: partners; Type: TABLE DATA; Schema: public; Owner: aquacms
--

COPY public.partners (id, short_id, slug, name, description, detailed_description, partner_category_id, location, since, image, contact_email, contact_phone, website, is_active, sort_order, created_at, updated_at) FROM stdin;
d62840f4-0e38-4a2c-be39-6ff2fcd27c3f	1	cong-ty-thuy-san-mien-tay	Công ty TNHH Thủy sản Miền Tây	Nhà phân phối thiết bị nuôi trồng thủy sản lớn nhất ĐBSCL.	\N	1b3b30af-c46b-405f-960e-9dab1e49fd7a	Cà Mau	2018	https://placehold.co/200x200/55B3D9/FFF?text=MT	\N	\N	\N	t	1	2026-05-05 02:45:38.089799+00	2026-05-05 02:45:38.0898+00
\.


--
-- Data for Name: posts; Type: TABLE DATA; Schema: public; Owner: aquacms
--

COPY public.posts (id, short_id, slug, title, excerpt, content, image, author, knowledge_category_id, read_time, is_published, published_at, view_count, meta_title, meta_desc, created_at, updated_at) FROM stdin;
17b97b0a-da30-457d-bbe3-150ae4fa3a40	1	5-buoc-chuan-bi-ao-nuoi-tom-dat-chuan	5 bước chuẩn bị ao nuôi tôm đạt chuẩn	Hướng dẫn chi tiết quy trình chuẩn bị ao nuôi tôm từ A-Z, đảm bảo tỷ lệ thành công cao nhất.	<h2>Bước 1: Cải tạo ao</h2><p>Sau mỗi vụ nuôi, ao cần được cải tạo kỹ lưỡng. Bao gồm tháo cạn nước, phơi đáy ao 7-10 ngày, bón vôi CaO với liều lượng 100-150 kg/1000m².</p><h2>Bước 2: Xử lý nước</h2><p>Cấp nước qua túi lọc, xử lý bằng Chlorine 30ppm, chạy quạt 2-3 ngày để bay hết dư lượng Chlorine.</p><h2>Bước 3: Gây màu nước</h2><p>Sử dụng vi sinh và phân bón sinh học để gây màu nước đạt độ trong 30-40cm.</p><h2>Bước 4: Kiểm tra chỉ tiêu</h2><p>pH: 7.5-8.5, Kiềm: 120-150 mg/L, DO > 5mg/L, NH3 < 0.1mg/L.</p><h2>Bước 5: Thả giống</h2><p>Thả giống vào sáng sớm hoặc chiều mát, mật độ 100-150 con/m².</p>	https://placehold.co/800x450/4CAF50/FFF?text=Chu%E1%BA%A9n+b%E1%BB%8B+ao	Admin	90ea5976-d6f0-4406-b11b-cbffe7e0bd28	5 phút	t	2026-04-30 02:45:38.019218+00	9	\N	\N	2026-05-05 02:45:38.018932+00	2026-05-12 10:01:19.780069+00
\.


--
-- Data for Name: product_contents; Type: TABLE DATA; Schema: public; Owner: aquacms
--

COPY public.product_contents (product_id, description, content_blocks, image, video_url) FROM stdin;
7eefff53-8079-4ec3-a756-b49ca1157a74	Quạt biến tần Tam Hoa FANTO sử dụng mô tơ biến tần tốc độ cao 200 R/MIN với trục inox 304 bền bỉ, giúp giảm tiêu thụ điện năng đến 30% và tăng lượng oxy hòa tan lên 30% so với quạt thông thường. Hệ thống bảo vệ động cơ thông minh hoạt động hiệu quả ngay cả khi điện yếu, kéo dài tuổi thọ thiết bị đáng kể.	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Công suất : 2.200 W\\r\\nVòng quay : 210 R/MIN\\r\\nLượng O₂ : 3,2 kg/h\\r\\nDiện tích phù hợp : 4.000 – 4.500 m²\\r\\nVật liệu : Trục Inox 304 | Mô tơ biến tần | Tiết kiệm điện 30%"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔  Giảm tiêu thụ điện năng đến 30% so với quạt thông thường\\r\\n  ✔  Lượng oxy tăng 30% so với sản phẩm thông thường\\r\\n  ✔  Bảo vệ động cơ khi điện yếu – kéo dài tuổi thọ thiết bị\\r\\n  ✔  Trục Inox 304 – Mô tơ biến tần tốc độ cao"}]	\N	\N
e6489dd6-926a-4f4b-9976-08e6252822b4	Quạt biến tần Tam Hoa FANTO sử dụng mô tơ biến tần tốc độ cao 200 R/MIN với trục inox 304 bền bỉ, giúp giảm tiêu thụ điện năng đến 30% và tăng lượng oxy hòa tan lên 30% so với quạt thông thường. Hệ thống bảo vệ động cơ thông minh hoạt động hiệu quả ngay cả khi điện yếu, kéo dài tuổi thọ thiết bị đáng kể.	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Công suất : 1.500 W\\r\\nVòng quay : 200 R/MIN\\r\\nLượng O₂ : 3,0 kg/h\\r\\nDiện tích phù hợp : 3.000 – 3.500 m²\\r\\nVật liệu : Trục Inox 304 | Mô tơ biến tần | Tiết kiệm điện 30%"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔  Giảm tiêu thụ điện năng đến 30% so với quạt thông thường\\r\\n  ✔  Lượng oxy tăng 30% so với sản phẩm thông thường\\r\\n  ✔  Bảo vệ động cơ khi điện yếu – kéo dài tuổi thọ thiết bị\\r\\n  ✔  Trục Inox 304 – Mô tơ biến tần tốc độ cao"}]	\N	\N
8fe19fa8-e4a7-4317-861c-cad29f35168c	Quạt biến tần Tam Hoa FANTO sử dụng mô tơ biến tần tốc độ cao 200 R/MIN với trục inox 304 bền bỉ, giúp giảm tiêu thụ điện năng đến 30% và tăng lượng oxy hòa tan lên 30% so với quạt thông thường. Hệ thống bảo vệ động cơ thông minh hoạt động hiệu quả ngay cả khi điện yếu, kéo dài tuổi thọ thiết bị đáng kể.	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Công suất : 1.100 W\\r\\nVòng quay : 200 R/MIN\\r\\nLượng O₂ : 2,8 kg/h\\r\\nDiện tích phù hợp : 3.000 – 3.500 m²\\r\\nVật liệu : Trục Inox 304 | Mô tơ biến tần | Tiết kiệm điện 30%"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔  Giảm tiêu thụ điện năng đến 30% so với quạt thông thường\\r\\n  ✔  Lượng oxy tăng 30% so với sản phẩm thông thường\\r\\n  ✔  Bảo vệ động cơ khi điện yếu – kéo dài tuổi thọ thiết bị\\r\\n  ✔  Trục Inox 304 – Mô tơ biến tần tốc độ cao"}]	\N	\N
38e89aab-89fd-4071-8158-14523fcf4224	\N	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Model : HRE40L\\r\\nCông suất : 1,5 kW\\r\\nLưu lượng : 1,6 M³/MIN\\r\\nÁp suất : 24,5 kPa\\r\\nDiện tích phù hợp : 2.000 – 3.000 m²\\r\\nỐng khí : 200 – 250 m"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔  Công nghệ sục khí mới – 5 ưu điểm vượt trội\\r\\n  ✔  Công suất oxy cao – tăng cường hoạt động của nước\\r\\n  ✔  Tiêu thụ điện năng cực thấp – hiệu quả chăn nuôi cao\\r\\n  ✔  An toàn – không lo rò điện xuống nguồn nước\\r\\n  ✔  Bảo vệ môi trường – vận hành êm"}]	\N	\N
3bea6971-25f0-4511-baea-454f170c1c54	Máy cho cá ăn tự động FANTO giúp tiết kiệm thời gian và công sức cho người nuôi, cho ăn đều đặn đúng liều lượng, tránh lãng phí thức ăn gây ô nhiễm nguồn nước. Nhờ đó giảm đáng kể nhân công, tăng hiệu quả quản lý ao nuôi và đảm bảo cá tăng trưởng nhanh, hạn chế bệnh tật.	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Công suất : 150 W\\r\\nĐiện áp : 220 V\\r\\nKhối lượng cám : 75 Kg\\r\\nVật liệu : Thùng inox, chân HDPE | Bảng ĐK tự động"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔  Bộ điều khiển hoàn toàn tự động – thay thế cho ăn thủ công\\r\\n  ✔  Phù hợp ao hồ diện tích lớn nhỏ, mọi loại thuỷ hải sản và kích cỡ cám viên\\r\\n  ✔  Cấu trúc nhỏ gọn, thống nhất thời gian – khoảng cách phun thức ăn\\r\\n  ✔  Tiết kiệm thức ăn – không lãng phí gây ô nhiễm nguồn nước\\r\\n  ✔  Giảm sức lao động – thu hồi vốn trong 2–3 tháng sử dụng\\r\\n  ✔  Cho ăn đều đặn, đúng liều lượng – cá tăng trưởng nhanh, hạn chế bệnh"}]	\N	\N
05775c10-e473-4608-ab8d-3ff53156e9c0	Quạt nước tạo dòng FANTO cung cấp nguồn oxy dồi dào cho ao nuôi, giải phóng khí độc và điều hòa cân bằng các yếu tố môi trường. Thiết bị giúp phân bố đều thuốc và chế phẩm sinh học, gom tụ chất thải để làm sạch môi trường nước, từ đó tôm cá tăng cường hoạt động, nâng cao sức đề kháng và phát triển khỏe mạnh hơn.	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Công suất : 1,5 kW / 2HP\\r\\nĐiện áp : 380 V\\r\\nSố cánh : 4 cánh\\r\\nHiệu suất : ≥ 1,9 kg O₂/kWh\\r\\nDiện tích phù hợp : 3.000 – 4.000 m²\\r\\nVật liệu : Mô tơ hộp số gang | Phao HDPE | Cánh nylon | Trục/khung Inox 304"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔  Cung cấp đầy đủ oxy hoà tan trong ao nuôi\\r\\n  ✔  Tạo dòng chảy mạnh – luân chuyển nước, tránh phân tầng\\r\\n  ✔  Giải phóng khí độc – cân bằng các yếu tố môi trường\\r\\n  ✔  Gom tụ chất thải – làm sạch môi trường nước\\r\\n  ✔  Phân bố đều thuốc, chế phẩm hoá chất trong ao\\r\\n  ✔  Tăng cường hoạt động tôm cá – nâng cao năng suất"}]	\N	\N
06d8b45d-dcce-4f77-ae90-679bcafab342	\N	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Model : HRE65L\\r\\nCông suất : 3,0 kW\\r\\nLưu lượng : 3,6 M³/MIN\\r\\nÁp suất : 24,5 kPa\\r\\nDiện tích phù hợp : 6.000 – 9.000 m²\\r\\nỐng khí : 400 – 700 m"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔  Công nghệ sục khí mới – 5 ưu điểm vượt trội\\r\\n  ✔  Công suất oxy cao – tăng cường hoạt động của nước\\r\\n  ✔  Tiêu thụ điện năng cực thấp – hiệu quả chăn nuôi cao\\r\\n  ✔  An toàn – không lo rò điện xuống nguồn nước\\r\\n  ✔  Bảo vệ môi trường – vận hành êm"}]	\N	\N
41fc5308-7aa8-4e52-83e2-95e355739baf	\N	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Model : SC-1,5\\r\\nCông suất : 1,5 kW / 2HP\\r\\nĐiện áp : 220 V\\r\\nHiệu suất : > 0,8 kg O₂/kWh\\r\\nĐầu ra : Ø 80 mm"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔  Phun mưa tạo oxy – cấp khí từ bề mặt xuống ao\\r\\n  ✔  Tăng oxy hòa tan hiệu quả, phù hợp ao nuôi tôm cá\\r\\n  ✔  Kết cấu chắc chắn – vận hành ổn định, tiết kiệm điện"}]	\N	\N
89fc460e-ff9b-47f4-bbb8-03302d239bce	\N	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Model : SC-1,5\\r\\nCông suất : 1,5 kW / 2HP\\r\\nĐiện áp : 380 V\\r\\nHiệu suất : > 2,0 kgO₂/kWh\\r\\nDiện tích phù hợp : 3.000 – 4.000 m²"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔  Tạo sóng mạnh – đẩy nhanh trao đổi nước giàu oxy\\r\\n  ✔  Hạn chế quang hợp của tảo – tiêu thụ chất cặn bã hữu cơ\\r\\n  ✔  Làm sạch nước – tránh NH3, Nitrit và các chất độc hại\\r\\n  ✔  Dễ hòa tan oxy – điều chỉnh nhiệt độ trên và dưới ao"}]	\N	\N
b8c5dbc3-cd82-41ad-8849-a6c7ce66cbc4	\N	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Model : SC-0,75\\r\\nCông suất : 0,75 kW / 1HP\\r\\nĐiện áp : 380 V\\r\\nHiệu suất : > 1,25 kgO₂/kWh\\r\\nDiện tích phù hợp : 2.000 – 3.000 m²"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔  Tạo sóng mạnh – đẩy nhanh trao đổi nước giàu oxy\\r\\n  ✔  Hạn chế quang hợp của tảo – tiêu thụ chất cặn bã hữu cơ\\r\\n  ✔  Làm sạch nước – tránh NH3, Nitrit và các chất độc hại\\r\\n  ✔  Dễ hòa tan oxy – điều chỉnh nhiệt độ trên và dưới ao"}]	\N	\N
1e669665-12bb-48bb-bc5c-b50edb50611d	Bơm chìm Inox FANTO bơm nước ra vào ao dễ dàng, không cần mồi nước và không lo tắc rác nhờ khả năng bơm lưu lượng lớn lên đến 170 m³/h. Vỏ inox không han gỉ phù hợp với cả nước ao lẫn nước mặn, vận hành bền bỉ và tiết kiệm điện năng.	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Công suất : 4 kW\\r\\nLưu lượng : 170 m³/h\\r\\nĐầu ra : Ø 100 mm\\r\\nĐẩy cao : 11 m\\r\\nChất liệu : Inox không han gỉ – phù hợp nước ao, nước mặn"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔  Bơm nước ra vào trong ao – không cần mồi nước\\r\\n  ✔  Không lo tắc rác – bơm lưu lượng lớn\\r\\n  ✔  Vỏ Inox không han gỉ – phù hợp nước ao, nước mặn\\r\\n  ✔  Vận hành bền bỉ – tiết kiệm điện"}]	\N	\N
2087b8d0-749b-48e1-8572-6d916d78363d	Quạt nước tạo dòng FANTO cung cấp nguồn oxy dồi dào cho ao nuôi, giải phóng khí độc và điều hòa cân bằng các yếu tố môi trường. Thiết bị giúp phân bố đều thuốc và chế phẩm sinh học, gom tụ chất thải để làm sạch môi trường nước, từ đó tôm cá tăng cường hoạt động, nâng cao sức đề kháng và phát triển khỏe mạnh hơn.	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Công suất : 0,75 kW / 1HP\\r\\nĐiện áp : 380 V\\r\\nSố cánh : 4 cánh\\r\\nHiệu suất : ≥ 1,45 kg O₂/kWh\\r\\nDiện tích phù hợp : 1.000 – 2.000 m²\\r\\nVật liệu : Mô tơ hộp số gang | Phao HDPE | Cánh nylon | Trục/khung Inox 304"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔  Cung cấp đầy đủ oxy hoà tan trong ao nuôi\\r\\n  ✔  Tạo dòng chảy mạnh – luân chuyển nước, tránh phân tầng\\r\\n  ✔  Giải phóng khí độc – cân bằng các yếu tố môi trường\\r\\n  ✔  Gom tụ chất thải – làm sạch môi trường nước\\r\\n  ✔  Phân bố đều thuốc, chế phẩm hoá chất trong ao\\r\\n  ✔  Tăng cường hoạt động tôm cá – nâng cao năng suất"}]	\N	\N
35b9bd0e-7b18-4a66-82bc-a4d7190071bc	\N	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Model : SC-1,5\\r\\nCông suất : 1,5 kW / 2HP\\r\\nĐiện áp : 380 V\\r\\nHiệu suất : > 3,0 kgO₂/kWh\\r\\nDiện tích phù hợp : 5.000 – 7.000 m²"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔  Tạo sóng 2 chiều mạnh mẽ – đẩy nhanh trao đổi nước giàu oxy\\r\\n  ✔  Hạn chế quang hợp của tảo – tiêu thụ chất cặn bã hữu cơ\\r\\n  ✔  Làm sạch nước – tránh NH3, Nitrit và các chất độc hại\\r\\n  ✔  Dễ hòa tan oxy – điều chỉnh nhiệt độ trên và dưới ao"}]	\N	\N
3dbe43c6-efda-47ce-af8c-de4c41934ce4	Máy trộn cám Inox FANTO giúp người nuôi tiết kiệm thời gian và công sức, trộn đều tất cả các loại thức ăn dạng viên cho tôm, cá, gia súc, gia cầm, kết hợp thuốc và vi sinh trực tiếp trong quá trình trộn. Kết cấu vững chắc với chất liệu inox cao cấp chống ăn mòn, bền bỉ theo thời gian.	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Công suất : 1,1 kW\\r\\nĐiện áp : 220 V\\r\\nKích thước : 45×65×90 cm\\r\\nDung tích : ~50 lít\\r\\nVật liệu : Inox 201"}, {"type": "heading", "content": "Tính năng nội bật"}, {"type": "paragraph", "content": "✔  Tiết kiệm thời gian và công sức cho người nuôi\\r\\n  ✔  Giảm chi phí nhân công đáng kể\\r\\n  ✔  Trộn nhanh, đều thức ăn với thuốc, vi sinh\\r\\n  ✔  Dễ dàng cho thức ăn vào và lấy thức ăn ra\\r\\n  ✔  Trộn được các loại thức ăn cho cá, tôm, gia súc, gia cầm\\r\\n  ✔  Kết cấu vững chắc, chất liệu Inox cao cấp chống ăn mòn"}]	\N	\N
4e49337d-8e39-4963-b95b-d05e0edfeff2	Quạt nước tạo dòng FANTO cung cấp nguồn oxy dồi dào cho ao nuôi, giải phóng khí độc và điều hòa cân bằng các yếu tố môi trường. Thiết bị giúp phân bố đều thuốc và chế phẩm sinh học, gom tụ chất thải để làm sạch môi trường nước, từ đó tôm cá tăng cường hoạt động, nâng cao sức đề kháng và phát triển khỏe mạnh hơn.	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Công suất : 0,75 kW / 1HP\\r\\nĐiện áp : 220 V\\r\\nSố cánh : 4 cánh\\r\\nHiệu suất : ≥ 1,45 kg O₂/kWh\\r\\nDiện tích phù hợp : 1.000 – 2.000 m²\\r\\nVật liệu : Mô tơ hộp số gang | Phao HDPE | Cánh nylon | Trục/khung Inox 304"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔  Cung cấp đầy đủ oxy hoà tan trong ao nuôi\\r\\n  ✔  Tạo dòng chảy mạnh – luân chuyển nước, tránh phân tầng\\r\\n  ✔  Giải phóng khí độc – cân bằng các yếu tố môi trường\\r\\n  ✔  Gom tụ chất thải – làm sạch môi trường nước\\r\\n  ✔  Phân bố đều thuốc, chế phẩm hoá chất trong ao\\r\\n  ✔  Tăng cường hoạt động tôm cá – nâng cao năng suất"}]	\N	\N
4f8a2f9d-f42a-44b2-bd5b-d24def6d0501	Máy cho cá ăn tự động FANTO giúp tiết kiệm thời gian và công sức cho người nuôi, cho ăn đều đặn đúng liều lượng, tránh lãng phí thức ăn gây ô nhiễm nguồn nước. Nhờ đó giảm đáng kể nhân công, tăng hiệu quả quản lý ao nuôi và đảm bảo cá tăng trưởng nhanh, hạn chế bệnh tật.	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Công suất : 5,1 kW\\r\\nĐiện áp : 380 V\\r\\nKhối lượng cám : 300 Kg\\r\\nVật liệu : Inox 201 | Hộp ĐK thông minh"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔  Bộ điều khiển hoàn toàn tự động – thay thế cho ăn thủ công\\r\\n  ✔  Phù hợp ao hồ diện tích lớn nhỏ, mọi loại thuỷ hải sản và kích cỡ cám viên\\r\\n  ✔  Cấu trúc nhỏ gọn, thống nhất thời gian – khoảng cách phun thức ăn\\r\\n  ✔  Tiết kiệm thức ăn – không lãng phí gây ô nhiễm nguồn nước\\r\\n  ✔  Giảm sức lao động – thu hồi vốn trong 2–3 tháng sử dụng\\r\\n  ✔  Cho ăn đều đặn, đúng liều lượng – cá tăng trưởng nhanh, hạn chế bệnh"}]	\N	\N
532bf983-e4a1-4992-9987-283997afd523	Quạt nước tạo dòng FANTO cung cấp nguồn oxy dồi dào cho ao nuôi, giải phóng khí độc và điều hòa cân bằng các yếu tố môi trường. Thiết bị giúp phân bố đều thuốc và chế phẩm sinh học, gom tụ chất thải để làm sạch môi trường nước, từ đó tôm cá tăng cường hoạt động, nâng cao sức đề kháng và phát triển khỏe mạnh hơn.	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Công suất : 0,75 kW / 1HP\\r\\nĐiện áp : 220 V\\r\\nSố cánh : 2 cánh\\r\\nHiệu suất : ≥ 1,25 kg O₂/kWh\\r\\nDiện tích phù hợp : 1.000 – 2.000 m²\\r\\nVật liệu : Mô tơ hộp số gang | Phao HDPE | Cánh nylon | Trục/khung Inox 304"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔  Cung cấp đầy đủ oxy hoà tan trong ao nuôi\\r\\n  ✔  Tạo dòng chảy mạnh – luân chuyển nước, tránh phân tầng\\r\\n  ✔  Giải phóng khí độc – cân bằng các yếu tố môi trường\\r\\n  ✔  Gom tụ chất thải – làm sạch môi trường nước\\r\\n  ✔  Phân bố đều thuốc, chế phẩm hoá chất trong ao\\r\\n  ✔  Tăng cường hoạt động tôm cá – nâng cao năng suất"}]	\N	\N
54c129cb-a71b-40ed-a22e-691ea6ce19b0	Quạt nước tạo dòng FANTO cung cấp nguồn oxy dồi dào cho ao nuôi, giải phóng khí độc và điều hòa cân bằng các yếu tố môi trường. Thiết bị giúp phân bố đều thuốc và chế phẩm sinh học, gom tụ chất thải để làm sạch môi trường nước, từ đó tôm cá tăng cường hoạt động, nâng cao sức đề kháng và phát triển khỏe mạnh hơn.	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Công suất : 2,2 kW / 3HP\\r\\nĐiện áp : 380 V\\r\\nSố cánh : 6 cánh\\r\\nHiệu suất : ≥ 2,6 kg O₂/kWh\\r\\nDiện tích phù hợp : ≥ 5.000 m²\\r\\nVật liệu : Mô tơ hộp số gang | Phao HDPE | Cánh nylon | Trục/khung Inox 304"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔  Cung cấp đầy đủ oxy hoà tan trong ao nuôi\\r\\n  ✔  Tạo dòng chảy mạnh – luân chuyển nước, tránh phân tầng\\r\\n  ✔  Giải phóng khí độc – cân bằng các yếu tố môi trường\\r\\n  ✔  Gom tụ chất thải – làm sạch môi trường nước\\r\\n  ✔  Phân bố đều thuốc, chế phẩm hoá chất trong ao\\r\\n  ✔  Tăng cường hoạt động tôm cá – nâng cao năng suất"}]	\N	\N
5965f620-ab47-46dd-be81-b0fd9c1aa2ae	Máy cho cá ăn tự động FANTO giúp tiết kiệm thời gian và công sức cho người nuôi, cho ăn đều đặn đúng liều lượng, tránh lãng phí thức ăn gây ô nhiễm nguồn nước. Nhờ đó giảm đáng kể nhân công, tăng hiệu quả quản lý ao nuôi và đảm bảo cá tăng trưởng nhanh, hạn chế bệnh tật.	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Công suất : 4,3 kW\\r\\nĐiện áp : 380 V\\r\\nKhối lượng cám : 300 Kg\\r\\nVật liệu : Inox 201 | Hộp ĐK thông minh"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔  Bộ điều khiển hoàn toàn tự động – thay thế cho ăn thủ công\\r\\n  ✔  Phù hợp ao hồ diện tích lớn nhỏ, mọi loại thuỷ hải sản và kích cỡ cám viên\\r\\n  ✔  Cấu trúc nhỏ gọn, thống nhất thời gian – khoảng cách phun thức ăn\\r\\n  ✔  Tiết kiệm thức ăn – không lãng phí gây ô nhiễm nguồn nước\\r\\n  ✔  Giảm sức lao động – thu hồi vốn trong 2–3 tháng sử dụng\\r\\n  ✔  Cho ăn đều đặn, đúng liều lượng – cá tăng trưởng nhanh, hạn chế bệnh"}]	\N	\N
6975bcfd-398b-404a-9849-8c5623b5396a	\N	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Model : SC-1,5\\r\\nCông suất : 1,5 kW / 2HP\\r\\nĐiện áp : 220 V\\r\\nHiệu suất : > 3,0 kgO₂/kWh\\r\\nDiện tích phù hợp : 5.000 – 7.000 m²"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔  Tạo sóng 2 chiều mạnh mẽ – đẩy nhanh trao đổi nước giàu oxy\\r\\n  ✔  Hạn chế quang hợp của tảo – tiêu thụ chất cặn bã hữu cơ\\r\\n  ✔  Làm sạch nước – tránh NH3, Nitrit và các chất độc hại\\r\\n  ✔  Dễ hòa tan oxy – điều chỉnh nhiệt độ trên và dưới ao"}]	\N	\N
6ad5fa9a-afdf-44e7-8d94-7355dc7207f5	Máy cho cá ăn tự động FANTO giúp tiết kiệm thời gian và công sức cho người nuôi, cho ăn đều đặn đúng liều lượng, tránh lãng phí thức ăn gây ô nhiễm nguồn nước. Nhờ đó giảm đáng kể nhân công, tăng hiệu quả quản lý ao nuôi và đảm bảo cá tăng trưởng nhanh, hạn chế bệnh tật.	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Công suất : 150 W\\r\\nĐiện áp : 220 V\\r\\nKhối lượng cám : 50 Kg\\r\\nVật liệu : Thùng nhựa HDPE | Bảng ĐK tự động"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔  Bộ điều khiển hoàn toàn tự động – thay thế cho ăn thủ công\\r\\n  ✔  Phù hợp ao hồ diện tích lớn nhỏ, mọi loại thuỷ hải sản và kích cỡ cám viên\\r\\n  ✔  Cấu trúc nhỏ gọn, thống nhất thời gian – khoảng cách phun thức ăn\\r\\n  ✔  Tiết kiệm thức ăn – không lãng phí gây ô nhiễm nguồn nước\\r\\n  ✔  Giảm sức lao động – thu hồi vốn trong 2–3 tháng sử dụng"}]	\N	\N
6d84884f-7db5-4167-85f1-4d8d9b0a97cd	Máy cho cá ăn tự động FANTO giúp tiết kiệm thời gian và công sức cho người nuôi, cho ăn đều đặn đúng liều lượng, tránh lãng phí thức ăn gây ô nhiễm nguồn nước. Nhờ đó giảm đáng kể nhân công, tăng hiệu quả quản lý ao nuôi và đảm bảo cá tăng trưởng nhanh, hạn chế bệnh tật.	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Công suất : 150 W\\r\\nĐiện áp : 220 V\\r\\nKhối lượng cám : 75 Kg\\r\\nVật liệu : Thùng nhựa HDPE | Bảng ĐK tự động"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔ Bộ điều khiển hoàn toàn tự động – thay thế cho ăn thủ công\\r\\n✔ Phù hợp ao hồ diện tích lớn nhỏ, mọi loại thuỷ hải sản và kích cỡ cám viên\\r\\n✔ Cấu trúc nhỏ gọn, thống nhất thời gian – khoảng cách phun thức ăn\\r\\n✔ Tiết kiệm thức ăn – không lãng phí gây ô nhiễm nguồn nước\\r\\n✔ Giảm sức lao động – thu hồi vốn trong 2–3 tháng sử dụng"}]	\N	\N
888d01db-fc92-4f94-adf1-c75427e52590	\N	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Model : HRE80L\\r\\nCông suất : 4,0 kW\\r\\nLưu lượng : 4,3 M³/MIN\\r\\nÁp suất : 26,5 kPa\\r\\nDiện tích phù hợp : 9.000 – 12.000 m²\\r\\nỐng khí : 700 – 1.000 m"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔  Công nghệ sục khí mới – 5 ưu điểm vượt trội\\r\\n  ✔  Công suất oxy cao – tăng cường hoạt động của nước\\r\\n  ✔  Tiêu thụ điện năng cực thấp – hiệu quả chăn nuôi cao\\r\\n  ✔  An toàn – không lo rò điện xuống nguồn nước\\r\\n  ✔  Bảo vệ môi trường – vận hành êm"}]	\N	\N
8ea1bfef-902a-4a1b-aa30-c251d9602eaf	Quạt nước tạo dòng FANTO cung cấp nguồn oxy dồi dào cho ao nuôi, giải phóng khí độc và điều hòa cân bằng các yếu tố môi trường. Thiết bị giúp phân bố đều thuốc và chế phẩm sinh học, gom tụ chất thải để làm sạch môi trường nước, từ đó tôm cá tăng cường hoạt động, nâng cao sức đề kháng và phát triển khỏe mạnh hơn.	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Công suất : 1,5 kW / 2HP\\r\\nĐiện áp : 220 V\\r\\nSố cánh : 4 cánh\\r\\nHiệu suất : ≥ 1,9 kg O₂/kWh\\r\\nDiện tích phù hợp : 2.000 – 3.000 m²\\r\\nVật liệu : Mô tơ hộp số gang | Phao HDPE | Cánh nylon | Trục/khung Inox 304"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔  Cung cấp đầy đủ oxy hoà tan trong ao nuôi\\r\\n  ✔  Tạo dòng chảy mạnh – luân chuyển nước, tránh phân tầng\\r\\n  ✔  Giải phóng khí độc – cân bằng các yếu tố môi trường\\r\\n  ✔  Gom tụ chất thải – làm sạch môi trường nước\\r\\n  ✔  Phân bố đều thuốc, chế phẩm hoá chất trong ao\\r\\n  ✔  Tăng cường hoạt động tôm cá – nâng cao năng suất"}]	\N	\N
95523a4d-f332-4bfb-8e3b-3fb598c8bab3	\N	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Model : SC-1,5\\r\\nCông suất : 1,5 kW / 2HP\\r\\nĐiện áp : 220 V\\r\\nHiệu suất : > 2,0 kgO₂/kWh\\r\\nDiện tích phù hợp : 3.000 – 4.000 m²"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔  Tạo sóng mạnh – đẩy nhanh trao đổi nước giàu oxy\\r\\n  ✔  Hạn chế quang hợp của tảo – tiêu thụ chất cặn bã hữu cơ\\r\\n  ✔  Làm sạch nước – tránh NH3, Nitrit và các chất độc hại\\r\\n  ✔  Dễ hòa tan oxy – điều chỉnh nhiệt độ trên và dưới ao"}]	\N	\N
9a7563bf-5ebc-4262-a308-845cc28927af	Quạt nước tạo dòng FANTO cung cấp nguồn oxy dồi dào cho ao nuôi, giải phóng khí độc và điều hòa cân bằng các yếu tố môi trường. Thiết bị giúp phân bố đều thuốc và chế phẩm sinh học, gom tụ chất thải để làm sạch môi trường nước, từ đó tôm cá tăng cường hoạt động, nâng cao sức đề kháng và phát triển khỏe mạnh hơn.	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Công suất : 0,75 kW / 1HP\\r\\nĐiện áp : 380 V\\r\\nSố cánh : 2 cánh\\r\\nHiệu suất : ≥ 1,25 kg O₂/kWh\\r\\nDiện tích phù hợp : 1.000 – 2.000 m²\\r\\nVật liệu : Mô tơ hộp số gang | Phao HDPE | Cánh nylon | Trục/khung Inox 304"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔  Cung cấp đầy đủ oxy hoà tan trong ao nuôi\\r\\n  ✔  Tạo dòng chảy mạnh – luân chuyển nước, tránh phân tầng\\r\\n  ✔  Giải phóng khí độc – cân bằng các yếu tố môi trường\\r\\n  ✔  Gom tụ chất thải – làm sạch môi trường nước\\r\\n  ✔  Phân bố đều thuốc, chế phẩm hoá chất trong ao\\r\\n  ✔  Tăng cường hoạt động tôm cá – nâng cao năng suất"}]	\N	\N
9d973d56-cb0e-43f3-be0c-3894b06f629d	\N	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Model : SC-0,75\\r\\nCông suất : 0,75 kW / 1HP\\r\\nĐiện áp : 220 V\\r\\nHiệu suất : > 1,25 kgO₂/kWh\\r\\nDiện tích phù hợp : 2.000 – 3.000 m²"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔  Tạo sóng mạnh – đẩy nhanh trao đổi nước giàu oxy\\r\\n  ✔  Hạn chế quang hợp của tảo – tiêu thụ chất cặn bã hữu cơ\\r\\n  ✔  Làm sạch nước – tránh NH3, Nitrit và các chất độc hại\\r\\n  ✔  Dễ hòa tan oxy – điều chỉnh nhiệt độ trên và dưới ao"}]	\N	\N
a32266ef-95dc-456c-b3b8-592a6bd7fd4f	Máy trộn cám Inox FANTO giúp người nuôi tiết kiệm thời gian và công sức, trộn đều tất cả các loại thức ăn dạng viên cho tôm, cá, gia súc, gia cầm, kết hợp thuốc và vi sinh trực tiếp trong quá trình trộn. Kết cấu vững chắc với chất liệu inox cao cấp chống ăn mòn, bền bỉ theo thời gian.	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Công suất : 1,1 kW\\r\\nĐiện áp : 220 V\\r\\nKích thước : 88 × 75 × 135 cm\\r\\nDung tích : ~150 lít\\r\\nVật liệu : Inox 201"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔  Tiết kiệm thời gian và công sức cho người nuôi\\r\\n  ✔  Giảm chi phí nhân công đáng kể\\r\\n  ✔  Trộn nhanh, đều thức ăn với thuốc, vi sinh\\r\\n  ✔  Dễ dàng cho thức ăn vào và lấy thức ăn ra\\r\\n  ✔  Trộn được các loại thức ăn cho cá, tôm, gia súc, gia cầm\\r\\n  ✔  Kết cấu vững chắc, chất liệu Inox cao cấp chống ăn mòn"}]	\N	\N
a6926cc9-e508-469a-983a-f489d23438a4	Máy cho cá ăn tự động FANTO giúp tiết kiệm thời gian và công sức cho người nuôi, cho ăn đều đặn đúng liều lượng, tránh lãng phí thức ăn gây ô nhiễm nguồn nước. Nhờ đó giảm đáng kể nhân công, tăng hiệu quả quản lý ao nuôi và đảm bảo cá tăng trưởng nhanh, hạn chế bệnh tật.	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Công suất : 150 W\\r\\nĐiện áp : 220 V\\r\\nKhối lượng cám : 100 Kg\\r\\nVật liệu : Thùng nhựa HDPE | Bảng ĐK tự động"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔  Bộ điều khiển hoàn toàn tự động – thay thế cho ăn thủ công\\r\\n  ✔  Phù hợp ao hồ diện tích lớn nhỏ, mọi loại thuỷ hải sản và kích cỡ cám viên\\r\\n  ✔  Cấu trúc nhỏ gọn, thống nhất thời gian – khoảng cách phun thức ăn\\r\\n  ✔  Tiết kiệm thức ăn – không lãng phí gây ô nhiễm nguồn nước\\r\\n  ✔  Giảm sức lao động – thu hồi vốn trong 2–3 tháng sử dụng\\r\\n  ✔  Cho ăn đều đặn, đúng liều lượng – cá tăng trưởng nhanh, hạn chế bệnh"}]	\N	\N
b21e2f12-fbf9-4db3-be8b-4b6a58ca6764	Máy siêu oxy FANTO với thiết kế cánh quạt 12 lá và tốc độ quay 160 vòng/phút tạo ra dòng nước tơi, mạnh mẽ, giúp oxy hòa tan cao và phân bố đều thuốc, chế phẩm sinh học trong toàn bộ ao. Thiết bị điều hòa cân bằng môi trường nước, gom tụ chất thải hiệu quả, giúp tôm cá tăng cường hoạt động, nâng cao sức đề kháng và cho năng suất vượt trội.	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Công suất : 1.300 W\\r\\nĐiện áp : 220V / 380V\\r\\nSố cánh : 4 cánh (12 lá)\\r\\nLượng O₂ : 2,85 kg/h\\r\\nDiện tích phù hợp : 3.500 – 5.000 m²\\r\\nVật liệu : Mô tơ vỏ Inox 304 | Hộp số bọc nhựa composite | Khung/trục Inox 304 | Cánh & Phao HDPE"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔  Động cơ siêu oxy bằng Inox 304 không han gỉ (vỏ, phớt, trục hộp số)\\r\\n  ✔  Hộp số bọc nhựa composite chống ăn mòn – phù hợp nước mặn\\r\\n  ✔  Khung trục Inox 304 không han gỉ\\r\\n  ✔  Cánh quạt 12 lá – tăng oxy hòa tan tối đa\\r\\n  ✔  Mô tơ tốc độ cao: oxy hòa tan cao, dòng chảy mạnh\\r\\n  ✔  Tiết kiệm điện năng – chi phí vận hành thấp"}]	\N	\N
b7c72ddd-c71e-4976-8266-40d3149423c3	Máy cho cá ăn tự động FANTO giúp tiết kiệm thời gian và công sức cho người nuôi, cho ăn đều đặn đúng liều lượng, tránh lãng phí thức ăn gây ô nhiễm nguồn nước. Nhờ đó giảm đáng kể nhân công, tăng hiệu quả quản lý ao nuôi và đảm bảo cá tăng trưởng nhanh, hạn chế bệnh tật.	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Công suất : 150 W\\r\\nĐiện áp : 220 V\\r\\nKhối lượng cám : 100 Kg\\r\\nVật liệu : Thùng inox, chân HDPE | Bảng ĐK tự động"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔  Bộ điều khiển hoàn toàn tự động – thay thế cho ăn thủ công\\r\\n  ✔  Phù hợp ao hồ diện tích lớn nhỏ, mọi loại thuỷ hải sản và kích cỡ cám viên\\r\\n  ✔  Cấu trúc nhỏ gọn, thống nhất thời gian – khoảng cách phun thức ăn\\r\\n  ✔  Tiết kiệm thức ăn – không lãng phí gây ô nhiễm nguồn nước\\r\\n  ✔  Giảm sức lao động – thu hồi vốn trong 2–3 tháng sử dụng\\r\\n  ✔  Cho ăn đều đặn, đúng liều lượng – cá tăng trưởng nhanh, hạn chế bệnh"}]	\N	\N
b7e4b5dd-c224-4850-aa29-acd6ac6d9583	Máy trộn cám Inox FANTO giúp người nuôi tiết kiệm thời gian và công sức, trộn đều tất cả các loại thức ăn dạng viên cho tôm, cá, gia súc, gia cầm, kết hợp thuốc và vi sinh trực tiếp trong quá trình trộn. Kết cấu vững chắc với chất liệu inox cao cấp chống ăn mòn, bền bỉ theo thời gian.	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Công suất : 1,1 kW\\r\\nĐiện áp : 220 V\\r\\nKích thước : 58×75×104 cm\\r\\nDung tích : ~75 lít\\r\\nVật liệu : Inox 201"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔ Tiết kiệm thời gian và công sức cho người nuôi\\r\\n✔ Giảm chi phí nhân công đáng kể\\r\\n✔ Trộn nhanh, đều thức ăn với thuốc, vi sinh\\r\\n✔ Dễ dàng cho thức ăn vào và lấy thức ăn ra\\r\\n✔ Trộn được các loại thức ăn cho cá, tôm, gia súc, gia cầm\\r\\n✔ Kết cấu vững chắc, chất liệu Inox cao cấp chống ăn mòn"}]	\N	\N
b85e5fbd-d9fa-4779-a904-f598679302b0	Máy siêu oxy FANTO với thiết kế cánh quạt 12 lá và tốc độ quay 160 vòng/phút tạo ra dòng nước tơi, mạnh mẽ, giúp oxy hòa tan cao và phân bố đều thuốc, chế phẩm sinh học trong toàn bộ ao. Thiết bị điều hòa cân bằng môi trường nước, gom tụ chất thải hiệu quả, giúp tôm cá tăng cường hoạt động, nâng cao sức đề kháng và cho năng suất vượt trội.	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Công suất : 2.200 W\\r\\nĐiện áp : 380 V\\r\\nSố cánh : 10 cánh (12 lá)\\r\\nLượng O₂ : 4,72 kg/h\\r\\nDiện tích phù hợp : ≥ 5.000 m²\\r\\nVật liệu : Mô tơ vỏ Inox 304 | Hộp số bọc nhựa composite | Khung/trục Inox 304 | Cánh & Phao HDPE"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔  Động cơ siêu oxy bằng Inox 304 không han gỉ (vỏ, phớt, trục hộp số)\\r\\n  ✔  Hộp số bọc nhựa composite chống ăn mòn – phù hợp nước mặn\\r\\n  ✔  Khung trục Inox 304 không han gỉ\\r\\n  ✔  Cánh quạt 12 lá – tăng oxy hòa tan tối đa\\r\\n  ✔  Mô tơ tốc độ cao: oxy hòa tan cao, dòng chảy mạnh\\r\\n  ✔  Tiết kiệm điện năng – chi phí vận hành thấp"}]	\N	\N
bd6721df-0e0a-48db-92ec-7c43c5d07a8f	Máy trộn cám Inox FANTO giúp người nuôi tiết kiệm thời gian và công sức, trộn đều tất cả các loại thức ăn dạng viên cho tôm, cá, gia súc, gia cầm, kết hợp thuốc và vi sinh trực tiếp trong quá trình trộn. Kết cấu vững chắc với chất liệu inox cao cấp chống ăn mòn, bền bỉ theo thời gian.	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Công suất : 1,1 kW\\r\\nĐiện áp : 220 V\\r\\nKích thước : 67 × 75 × 114 cm\\r\\nDung tích : ~100 lít\\r\\nVật liệu : Inox 201"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔ Tiết kiệm thời gian và công sức cho người nuôi\\r\\n✔ Giảm chi phí nhân công đáng kể\\r\\n✔ Trộn nhanh, đều thức ăn với thuốc, vi sinh\\r\\n✔ Dễ dàng cho thức ăn vào và lấy thức ăn ra\\r\\n✔ Trộn được các loại thức ăn cho cá, tôm, gia súc, gia cầm\\r\\n✔ Kết cấu vững chắc, chất liệu Inox cao cấp chống ăn mòn"}]	\N	\N
bdaf9356-92cb-4a4e-bf01-acf90d233946	Máy siêu oxy FANTO với thiết kế cánh quạt 12 lá và tốc độ quay 160 vòng/phút tạo ra dòng nước tơi, mạnh mẽ, giúp oxy hòa tan cao và phân bố đều thuốc, chế phẩm sinh học trong toàn bộ ao. Thiết bị điều hòa cân bằng môi trường nước, gom tụ chất thải hiệu quả, giúp tôm cá tăng cường hoạt động, nâng cao sức đề kháng và cho năng suất vượt trội.	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Công suất : 1.300 W\\r\\nĐiện áp : 220V / 380V\\r\\nSố cánh : 4 cánh (12 lá)\\r\\nLượng O₂ : 2,55 kg/h\\r\\nDiện tích phù hợp : 2.500 – 3.500 m²\\r\\nVật liệu : Mô tơ vỏ Inox 304 | Hộp số bọc nhựa composite | Khung/trục Inox 304 | Cánh & Phao HDPE"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔  Động cơ siêu oxy bằng Inox 304 không han gỉ (vỏ, phớt, trục hộp số)\\r\\n  ✔  Hộp số bọc nhựa composite chống ăn mòn – phù hợp nước mặn\\r\\n  ✔  Khung trục Inox 304 không han gỉ\\r\\n  ✔  Cánh quạt 12 lá – tăng oxy hòa tan tối đa\\r\\n  ✔  Mô tơ tốc độ cao: oxy hòa tan cao, dòng chảy mạnh\\r\\n  ✔  Tiết kiệm điện năng – chi phí vận hành thấp"}]	\N	\N
cb6c58a1-261c-4b7c-85a2-14e7af4ed9ee	Quạt nước tạo dòng FANTO cung cấp nguồn oxy dồi dào cho ao nuôi, giải phóng khí độc và điều hòa cân bằng các yếu tố môi trường. Thiết bị giúp phân bố đều thuốc và chế phẩm sinh học, gom tụ chất thải để làm sạch môi trường nước, từ đó tôm cá tăng cường hoạt động, nâng cao sức đề kháng và phát triển khỏe mạnh hơn.	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Công suất : 2,2 kW / 3HP\\r\\nĐiện áp : 220 V\\r\\nSố cánh : 6 cánh\\r\\nHiệu suất : ≥ 2,6 kg O₂/kWh\\r\\nDiện tích phù hợp : ≥ 5.000 m²\\r\\nVật liệu : Mô tơ hộp số gang | Phao HDPE | Cánh nylon | Trục/khung Inox 304"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔  Cung cấp đầy đủ oxy hoà tan trong ao nuôi\\r\\n  ✔  Tạo dòng chảy mạnh – luân chuyển nước, tránh phân tầng\\r\\n  ✔  Giải phóng khí độc – cân bằng các yếu tố môi trường\\r\\n  ✔  Gom tụ chất thải – làm sạch môi trường nước\\r\\n  ✔  Phân bố đều thuốc, chế phẩm hoá chất trong ao\\r\\n  ✔  Tăng cường hoạt động tôm cá – nâng cao năng suất"}]	\N	\N
d9557595-cb71-4e95-bf23-74c95aa06de1	Máy siêu oxy FANTO với thiết kế cánh quạt 12 lá và tốc độ quay 160 vòng/phút tạo ra dòng nước tơi, mạnh mẽ, giúp oxy hòa tan cao và phân bố đều thuốc, chế phẩm sinh học trong toàn bộ ao. Thiết bị điều hòa cân bằng môi trường nước, gom tụ chất thải hiệu quả, giúp tôm cá tăng cường hoạt động, nâng cao sức đề kháng và cho năng suất vượt trội.	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Công suất : 1.500 W\\r\\nĐiện áp : 380 V\\r\\nSố cánh : 6 cánh (12 lá)\\r\\nLượng O₂ : 3,72 kg/h\\r\\nDiện tích phù hợp : ≥ 5.000 m²\\r\\nVật liệu : Mô tơ vỏ Inox 304 | Hộp số bọc nhựa composite | Khung/trục Inox 304 | Cánh & Phao HDPE"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔  Động cơ siêu oxy bằng Inox 304 không han gỉ (vỏ, phớt, trục hộp số)\\r\\n  ✔  Hộp số bọc nhựa composite chống ăn mòn – phù hợp nước mặn\\r\\n  ✔  Khung trục Inox 304 không han gỉ\\r\\n  ✔  Cánh quạt 12 lá – tăng oxy hòa tan tối đa\\r\\n  ✔  Mô tơ tốc độ cao: oxy hòa tan cao, dòng chảy mạnh\\r\\n  ✔  Tiết kiệm điện năng – chi phí vận hành thấp"}]	\N	\N
dd8cea67-2423-45b4-9f6e-8c751c81cd50	Máy cho cá ăn tự động FANTO giúp tiết kiệm thời gian và công sức cho người nuôi, cho ăn đều đặn đúng liều lượng, tránh lãng phí thức ăn gây ô nhiễm nguồn nước. Nhờ đó giảm đáng kể nhân công, tăng hiệu quả quản lý ao nuôi và đảm bảo cá tăng trưởng nhanh, hạn chế bệnh tật.	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Công suất : 3,4 kW\\r\\nĐiện áp : 380 V\\r\\nKhối lượng cám : 150 Kg\\r\\nVật liệu : Inox 201 | Hộp ĐK thông minh"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔  Bộ điều khiển hoàn toàn tự động – thay thế cho ăn thủ công\\r\\n  ✔  Phù hợp ao hồ diện tích lớn nhỏ, mọi loại thuỷ hải sản và kích cỡ cám viên\\r\\n  ✔  Cấu trúc nhỏ gọn, thống nhất thời gian – khoảng cách phun thức ăn\\r\\n  ✔  Tiết kiệm thức ăn – không lãng phí gây ô nhiễm nguồn nước\\r\\n  ✔  Giảm sức lao động – thu hồi vốn trong 2–3 tháng sử dụng\\r\\n  ✔  Cho ăn đều đặn, đúng liều lượng – cá tăng trưởng nhanh, hạn chế bệnh"}]	\N	\N
e5d1e900-9660-49f9-b970-e6b027390dec	Máy siêu oxy FANTO với thiết kế cánh quạt 12 lá và tốc độ quay 160 vòng/phút tạo ra dòng nước tơi, mạnh mẽ, giúp oxy hòa tan cao và phân bố đều thuốc, chế phẩm sinh học trong toàn bộ ao. Thiết bị điều hòa cân bằng môi trường nước, gom tụ chất thải hiệu quả, giúp tôm cá tăng cường hoạt động, nâng cao sức đề kháng và cho năng suất vượt trội.	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Công suất : 1.500 W\\r\\nĐiện áp : 220 V\\r\\nSố cánh : 6 cánh (12 lá)\\r\\nLượng O₂ : 3,72 kg/h\\r\\nDiện tích phù hợp : ≥ 5.000 m²\\r\\nVật liệu : Mô tơ vỏ Inox 304 | Hộp số bọc nhựa composite | Khung/trục Inox 304 | Cánh & Phao HDPE"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔  Động cơ siêu oxy bằng Inox 304 không han gỉ (vỏ, phớt, trục hộp số)\\r\\n  ✔  Hộp số bọc nhựa composite chống ăn mòn – phù hợp nước mặn\\r\\n  ✔  Khung trục Inox 304 không han gỉ\\r\\n  ✔  Cánh quạt 12 lá – tăng oxy hòa tan tối đa\\r\\n  ✔  Mô tơ tốc độ cao: oxy hòa tan cao, dòng chảy mạnh\\r\\n  ✔  Tiết kiệm điện năng – chi phí vận hành thấp"}]	\N	\N
e7297c50-b3b0-418a-8f8a-350030b31cdf	\N	[{"type": "heading", "content": "Thông số sản phẩm"}, {"type": "paragraph", "content": "Model : HRE50L\\r\\nCông suất : 2,2 kW\\r\\nLưu lượng : 2,6 M³/MIN\\r\\nÁp suất : 24,5 kPa\\r\\nDiện tích phù hợp : 3.000 – 6.000 m²\\r\\nỐng khí : 250 – 400 m"}, {"type": "heading", "content": "Tính năng nổi bật"}, {"type": "paragraph", "content": "✔  Công nghệ sục khí mới – 5 ưu điểm vượt trội\\r\\n  ✔  Công suất oxy cao – tăng cường hoạt động của nước\\r\\n  ✔  Tiêu thụ điện năng cực thấp – hiệu quả chăn nuôi cao\\r\\n  ✔  An toàn – không lo rò điện xuống nguồn nước\\r\\n  ✔  Bảo vệ môi trường – vận hành êm"}]	\N	\N
\.


--
-- Data for Name: product_finances; Type: TABLE DATA; Schema: public; Owner: aquacms
--

COPY public.product_finances (product_id, price, show_price, is_featured) FROM stdin;
3dbe43c6-efda-47ce-af8c-de4c41934ce4	5700000	t	f
b7e4b5dd-c224-4850-aa29-acd6ac6d9583	5900000	t	f
bd6721df-0e0a-48db-92ec-7c43c5d07a8f	6200000	t	f
a32266ef-95dc-456c-b3b8-592a6bd7fd4f	8197000	t	f
6ad5fa9a-afdf-44e7-8d94-7355dc7207f5	3300000	t	f
6d84884f-7db5-4167-85f1-4d8d9b0a97cd	4100000	t	f
05775c10-e473-4608-ab8d-3ff53156e9c0	6700000	t	f
06d8b45d-dcce-4f77-ae90-679bcafab342	4600000	t	f
1e669665-12bb-48bb-bc5c-b50edb50611d	\N	t	f
2087b8d0-749b-48e1-8572-6d916d78363d	6500000	t	f
35b9bd0e-7b18-4a66-82bc-a4d7190071bc	8400000	t	f
38e89aab-89fd-4071-8158-14523fcf4224	\N	t	f
3bea6971-25f0-4511-baea-454f170c1c54	4800000	t	f
41fc5308-7aa8-4e52-83e2-95e355739baf	1900000	t	f
4e49337d-8e39-4963-b95b-d05e0edfeff2	6300000	t	f
4f8a2f9d-f42a-44b2-bd5b-d24def6d0501	\N	t	f
532bf983-e4a1-4992-9987-283997afd523	6000000	t	f
54c129cb-a71b-40ed-a22e-691ea6ce19b0	8300000	t	f
5965f620-ab47-46dd-be81-b0fd9c1aa2ae	37000000	t	f
6975bcfd-398b-404a-9849-8c5623b5396a	8600000	t	f
7eefff53-8079-4ec3-a756-b49ca1157a74	\N	t	f
888d01db-fc92-4f94-adf1-c75427e52590	\N	t	f
89fc460e-ff9b-47f4-bbb8-03302d239bce	6200000	t	f
8ea1bfef-902a-4a1b-aa30-c251d9602eaf	6500000	t	f
8fe19fa8-e4a7-4317-861c-cad29f35168c	\N	t	f
95523a4d-f332-4bfb-8e3b-3fb598c8bab3	6400000	t	f
9a7563bf-5ebc-4262-a308-845cc28927af	5800000	t	f
9d973d56-cb0e-43f3-be0c-3894b06f629d	6200000	t	f
a6926cc9-e508-469a-983a-f489d23438a4	4500000	t	f
b21e2f12-fbf9-4db3-be8b-4b6a58ca6764	8300000	t	f
b7c72ddd-c71e-4976-8266-40d3149423c3	5000000	t	f
b85e5fbd-d9fa-4779-a904-f598679302b0	12500000	t	f
b8c5dbc3-cd82-41ad-8849-a6c7ce66cbc4	6000000	t	f
bdaf9356-92cb-4a4e-bf01-acf90d233946	8600000	t	f
cb6c58a1-261c-4b7c-85a2-14e7af4ed9ee	8500000	t	f
d9557595-cb71-4e95-bf23-74c95aa06de1	9800000	t	f
dd8cea67-2423-45b4-9f6e-8c751c81cd50	\N	t	f
e5d1e900-9660-49f9-b970-e6b027390dec	10000000	t	f
e6489dd6-926a-4f4b-9976-08e6252822b4	\N	t	f
e7297c50-b3b0-418a-8f8a-350030b31cdf	3600000	t	f
\.


--
-- Data for Name: product_metadata; Type: TABLE DATA; Schema: public; Owner: aquacms
--

COPY public.product_metadata (product_id, slug, meta_title, meta_desc) FROM stdin;
3dbe43c6-efda-47ce-af8c-de4c41934ce4	may-tron-cam-fanto-1b-50l	\N	\N
b7e4b5dd-c224-4850-aa29-acd6ac6d9583	may-tron-cam-fanto-2b-75l	\N	\N
bd6721df-0e0a-48db-92ec-7c43c5d07a8f	may-tron-cam-fanto-3b-100l	\N	\N
a32266ef-95dc-456c-b3b8-592a6bd7fd4f	may-tron-cam-fanto-4b-150l	\N	\N
6ad5fa9a-afdf-44e7-8d94-7355dc7207f5	may-cho-ca-an-2-bao-nhua-fanto	\N	\N
6d84884f-7db5-4167-85f1-4d8d9b0a97cd	may-cho-ca-an-3-bao-nhua-fanto	\N	\N
05775c10-e473-4608-ab8d-3ff53156e9c0	quat-nuoc-tao-dong-fanto-4-canh-2hp-380v	\N	\N
06d8b45d-dcce-4f77-ae90-679bcafab342	may-thoi-khi-duoi-nuoc-fanto-hre65l	\N	\N
1e669665-12bb-48bb-bc5c-b50edb50611d	bom-chim-inox-fanto-4kw	\N	\N
2087b8d0-749b-48e1-8572-6d916d78363d	quat-nuoc-tao-dong-fanto-4-canh-1hp-380v	\N	\N
35b9bd0e-7b18-4a66-82bc-a4d7190071bc	may-tao-song-2-tac-dung-fanto-sc15-380v	\N	\N
38e89aab-89fd-4071-8158-14523fcf4224	may-thoi-khi-duoi-nuoc-fanto-hre40l	\N	\N
3bea6971-25f0-4511-baea-454f170c1c54	may-cho-ca-an-3-bao-inox-fanto	\N	\N
41fc5308-7aa8-4e52-83e2-95e355739baf	bom-mua-fanto-sc15-220v	\N	\N
4e49337d-8e39-4963-b95b-d05e0edfeff2	quat-nuoc-tao-dong-fanto-4-canh-1hp-220v	\N	\N
4f8a2f9d-f42a-44b2-bd5b-d24def6d0501	may-cho-ca-an-12-bao-inox-360-2-voi-fanto	\N	\N
532bf983-e4a1-4992-9987-283997afd523	quat-nuoc-tao-dong-fanto-2-canh-1hp-220v	\N	\N
54c129cb-a71b-40ed-a22e-691ea6ce19b0	quat-nuoc-tao-dong-fanto-6-canh-3hp-380v	\N	\N
5965f620-ab47-46dd-be81-b0fd9c1aa2ae	may-cho-ca-an-12-bao-inox-360-1-voi-fanto	\N	\N
6975bcfd-398b-404a-9849-8c5623b5396a	may-tao-song-2-tac-dung-fanto-sc15-220v	\N	\N
7eefff53-8079-4ec3-a756-b49ca1157a74	quat-bien-tan-tam-hoa-fanto-2200w	\N	\N
888d01db-fc92-4f94-adf1-c75427e52590	may-thoi-khi-duoi-nuoc-fanto-hre80l	\N	\N
89fc460e-ff9b-47f4-bbb8-03302d239bce	may-tao-song-1-tac-dung-fanto-sc15-380v	\N	\N
8ea1bfef-902a-4a1b-aa30-c251d9602eaf	quat-nuoc-tao-dong-fanto-4-canh-2hp-220v	\N	\N
8fe19fa8-e4a7-4317-861c-cad29f35168c	quat-bien-tan-tam-hoa-fanto-1100w	\N	\N
95523a4d-f332-4bfb-8e3b-3fb598c8bab3	may-tao-song-1-tac-dung-fanto-sc15-220v	\N	\N
9a7563bf-5ebc-4262-a308-845cc28927af	quat-nuoc-tao-dong-fanto-2-canh-1hp-380v	\N	\N
9d973d56-cb0e-43f3-be0c-3894b06f629d	may-tao-song-1-tac-dung-fanto-sc075-220v	\N	\N
a6926cc9-e508-469a-983a-f489d23438a4	may-cho-ca-an-4-bao-nhua-fanto	\N	\N
b21e2f12-fbf9-4db3-be8b-4b6a58ca6764	may-sieu-oxy-fanto-4-canh-1300w-b	\N	\N
b7c72ddd-c71e-4976-8266-40d3149423c3	may-cho-ca-an-4-bao-inox-fanto	\N	\N
b85e5fbd-d9fa-4779-a904-f598679302b0	may-sieu-oxy-fanto-10-canh-2200w-380v	\N	\N
b8c5dbc3-cd82-41ad-8849-a6c7ce66cbc4	may-tao-song-1-tac-dung-fanto-sc075-380v	\N	\N
bdaf9356-92cb-4a4e-bf01-acf90d233946	may-sieu-oxy-fanto-4-canh-1300w-a	\N	\N
cb6c58a1-261c-4b7c-85a2-14e7af4ed9ee	quat-nuoc-tao-dong-fanto-6-canh-3hp-220v	\N	\N
d9557595-cb71-4e95-bf23-74c95aa06de1	may-sieu-oxy-fanto-6-canh-1500w-380v	\N	\N
dd8cea67-2423-45b4-9f6e-8c751c81cd50	may-cho-ca-an-6-bao-inox-xoay-360-fanto	\N	\N
e5d1e900-9660-49f9-b970-e6b027390dec	may-sieu-oxy-fanto-6-canh-1500w-220v	\N	\N
e6489dd6-926a-4f4b-9976-08e6252822b4	quat-bien-tan-tam-hoa-fanto-1500w	\N	\N
e7297c50-b3b0-418a-8f8a-350030b31cdf	may-thoi-khi-duoi-nuoc-fanto-hre50l	\N	\N
\.


--
-- Data for Name: product_statistics; Type: TABLE DATA; Schema: public; Owner: aquacms
--

COPY public.product_statistics (product_id, view_count) FROM stdin;
3dbe43c6-efda-47ce-af8c-de4c41934ce4	10
41fc5308-7aa8-4e52-83e2-95e355739baf	4
d9557595-cb71-4e95-bf23-74c95aa06de1	1
1e669665-12bb-48bb-bc5c-b50edb50611d	10
05775c10-e473-4608-ab8d-3ff53156e9c0	0
2087b8d0-749b-48e1-8572-6d916d78363d	0
38e89aab-89fd-4071-8158-14523fcf4224	0
532bf983-e4a1-4992-9987-283997afd523	0
54c129cb-a71b-40ed-a22e-691ea6ce19b0	0
5965f620-ab47-46dd-be81-b0fd9c1aa2ae	0
7eefff53-8079-4ec3-a756-b49ca1157a74	0
888d01db-fc92-4f94-adf1-c75427e52590	0
89fc460e-ff9b-47f4-bbb8-03302d239bce	0
8ea1bfef-902a-4a1b-aa30-c251d9602eaf	0
8fe19fa8-e4a7-4317-861c-cad29f35168c	0
95523a4d-f332-4bfb-8e3b-3fb598c8bab3	0
9a7563bf-5ebc-4262-a308-845cc28927af	0
9d973d56-cb0e-43f3-be0c-3894b06f629d	0
a6926cc9-e508-469a-983a-f489d23438a4	0
b21e2f12-fbf9-4db3-be8b-4b6a58ca6764	0
b7c72ddd-c71e-4976-8266-40d3149423c3	0
bdaf9356-92cb-4a4e-bf01-acf90d233946	0
cb6c58a1-261c-4b7c-85a2-14e7af4ed9ee	0
e5d1e900-9660-49f9-b970-e6b027390dec	0
e6489dd6-926a-4f4b-9976-08e6252822b4	0
b85e5fbd-d9fa-4779-a904-f598679302b0	1
06d8b45d-dcce-4f77-ae90-679bcafab342	1
e7297c50-b3b0-418a-8f8a-350030b31cdf	1
b8c5dbc3-cd82-41ad-8849-a6c7ce66cbc4	1
a32266ef-95dc-456c-b3b8-592a6bd7fd4f	3
4f8a2f9d-f42a-44b2-bd5b-d24def6d0501	1
4e49337d-8e39-4963-b95b-d05e0edfeff2	1
dd8cea67-2423-45b4-9f6e-8c751c81cd50	1
6d84884f-7db5-4167-85f1-4d8d9b0a97cd	1
3bea6971-25f0-4511-baea-454f170c1c54	2
bd6721df-0e0a-48db-92ec-7c43c5d07a8f	2
b7e4b5dd-c224-4850-aa29-acd6ac6d9583	6
6ad5fa9a-afdf-44e7-8d94-7355dc7207f5	6
6975bcfd-398b-404a-9849-8c5623b5396a	1
35b9bd0e-7b18-4a66-82bc-a4d7190071bc	4
\.


--
-- Data for Name: products; Type: TABLE DATA; Schema: public; Owner: aquacms
--

COPY public.products (id, short_id, sku, name, category_id, status, created_at, updated_at) FROM stdin;
05775c10-e473-4608-ab8d-3ff53156e9c0	15	FANTO-QN-4C-2HP-380V	Quạt nước tạo dòng FANTO 4 cánh 2HP 380V	d010f78f-b1a6-4244-9590-b6b34703c894	available	2026-05-07 06:46:10.011867+00	2026-05-07 09:43:55.102209+00
06d8b45d-dcce-4f77-ae90-679bcafab342	16	FANTO-HRE65L	Máy thổi khí dưới nước FANTO HRE65L	a8e339cc-2954-42c6-ba32-5a2353383dfe	available	2026-05-07 06:46:10.083763+00	2026-05-07 09:43:55.102209+00
1e669665-12bb-48bb-bc5c-b50edb50611d	17	FANTO-BCM-4KW	Bơm chìm Inox FANTO 4kW	4ec8697d-aef1-4210-bd51-b9c40e3fcaa0	available	2026-05-07 06:46:10.090584+00	2026-05-07 09:43:55.102209+00
2087b8d0-749b-48e1-8572-6d916d78363d	18	FANTO-QN-4C-1HP-380V	Quạt nước tạo dòng FANTO 4 cánh 1HP 380V	d010f78f-b1a6-4244-9590-b6b34703c894	available	2026-05-07 06:46:10.005172+00	2026-05-07 09:43:55.102209+00
35b9bd0e-7b18-4a66-82bc-a4d7190071bc	19	FANTO-SC2-15-380V	Máy tạo sóng 2 tác dụng FANTO SC-1,5 380V	3143e24c-2fb8-4899-9460-8e975d8722c7	available	2026-05-07 06:46:10.073608+00	2026-05-07 09:43:55.102209+00
38e89aab-89fd-4071-8158-14523fcf4224	20	FANTO-HRE40L	Máy thổi khí dưới nước FANTO HRE40L	a8e339cc-2954-42c6-ba32-5a2353383dfe	available	2026-05-07 06:46:10.076838+00	2026-05-07 09:43:55.102209+00
3bea6971-25f0-4511-baea-454f170c1c54	21	FANTO-3-IN	Máy cho cá ăn 3 bao inox FANTO	531a00d4-ad41-42a5-89b6-b3ddf6016c8e	available	2026-05-07 06:46:09.975797+00	2026-05-07 09:43:55.102209+00
3dbe43c6-efda-47ce-af8c-de4c41934ce4	9	FANTO-1B-50L	Máy trộn cám FANTO 1B\\50L	f6ff294a-eb36-47b3-82e3-acf886b20e0f	available	2026-05-07 05:15:44.124543+00	2026-05-07 09:43:55.102209+00
41fc5308-7aa8-4e52-83e2-95e355739baf	22	FANTO-BM-SC15-220V	Bơm mưa FANTO SC-1,5 220V	9676fe3c-1c74-4281-a7c2-2f40282ca3c7	available	2026-05-07 06:46:10.094231+00	2026-05-07 09:43:55.102209+00
4e49337d-8e39-4963-b95b-d05e0edfeff2	23	FANTO-QN-4C-1HP-220V	Quạt nước tạo dòng FANTO 4 cánh 1HP 220V	d010f78f-b1a6-4244-9590-b6b34703c894	available	2026-05-07 06:46:10.001634+00	2026-05-07 09:43:55.102209+00
4f8a2f9d-f42a-44b2-bd5b-d24def6d0501	24	FANTO-12-IN-360-2V	Máy cho cá ăn 12 bao inox 360° 2 vòi FANTO	531a00d4-ad41-42a5-89b6-b3ddf6016c8e	available	2026-05-07 06:46:09.99112+00	2026-05-07 09:43:55.102209+00
532bf983-e4a1-4992-9987-283997afd523	25	FANTO-QN-2C-1HP-220V	Quạt nước tạo dòng FANTO 2 cánh 1HP 220V	d010f78f-b1a6-4244-9590-b6b34703c894	available	2026-05-07 06:46:09.994923+00	2026-05-07 09:43:55.102209+00
54c129cb-a71b-40ed-a22e-691ea6ce19b0	26	FANTO-QN-6C-3HP-380V	Quạt nước tạo dòng FANTO 6 cánh 3HP 380V	d010f78f-b1a6-4244-9590-b6b34703c894	available	2026-05-07 06:46:10.019541+00	2026-05-07 09:43:55.102209+00
5965f620-ab47-46dd-be81-b0fd9c1aa2ae	27	FANTO-12-IN-360-1V	Máy cho cá ăn 12 bao inox 360° 1 vòi FANTO	531a00d4-ad41-42a5-89b6-b3ddf6016c8e	available	2026-05-07 06:46:09.987508+00	2026-05-07 09:43:55.102209+00
6975bcfd-398b-404a-9849-8c5623b5396a	28	FANTO-SC2-15-220V	Máy tạo sóng 2 tác dụng FANTO SC-1,5 220V	3143e24c-2fb8-4899-9460-8e975d8722c7	available	2026-05-07 06:46:10.070296+00	2026-05-07 09:43:55.102209+00
6ad5fa9a-afdf-44e7-8d94-7355dc7207f5	13	FANTO-2-BN	Máy cho cá ăn 2 bao nhựa FANTO	531a00d4-ad41-42a5-89b6-b3ddf6016c8e	available	2026-05-07 05:41:20.352874+00	2026-05-07 09:43:55.102209+00
7eefff53-8079-4ec3-a756-b49ca1157a74	29	FANTO-THBT-2200W	Quạt biến tần Tam Hoa FANTO 2.200W	22419e79-f027-4cbc-a59a-74808a6e740b	available	2026-05-07 06:46:10.0503+00	2026-05-07 09:52:09.268298+00
e6489dd6-926a-4f4b-9976-08e6252822b4	47	FANTO-THBT-1500W	Quạt biến tần Tam Hoa FANTO 1.500W	22419e79-f027-4cbc-a59a-74808a6e740b	available	2026-05-07 06:46:10.046766+00	2026-05-07 09:52:17.072398+00
8fe19fa8-e4a7-4317-861c-cad29f35168c	33	FANTO-THBT-1100W	Quạt biến tần Tam Hoa FANTO 1.100W	22419e79-f027-4cbc-a59a-74808a6e740b	available	2026-05-07 06:46:10.041666+00	2026-05-07 09:52:25.131178+00
6d84884f-7db5-4167-85f1-4d8d9b0a97cd	14	FANTO-3-BN	Máy cho cá ăn 3 bao nhựa FANTO	531a00d4-ad41-42a5-89b6-b3ddf6016c8e	available	2026-05-07 05:45:27.306963+00	2026-05-07 09:43:55.102209+00
888d01db-fc92-4f94-adf1-c75427e52590	30	FANTO-HRE80L	Máy thổi khí dưới nước FANTO HRE80L	a8e339cc-2954-42c6-ba32-5a2353383dfe	available	2026-05-07 06:46:10.087402+00	2026-05-07 09:43:55.102209+00
89fc460e-ff9b-47f4-bbb8-03302d239bce	31	FANTO-SC15-380V	Máy tạo sóng 1 tác dụng FANTO SC-1,5 380V	a8a8c4e1-e3a2-4e11-96ed-b02c9cfd068e	available	2026-05-07 06:46:10.066243+00	2026-05-07 09:43:55.102209+00
8ea1bfef-902a-4a1b-aa30-c251d9602eaf	32	FANTO-QN-4C-2HP-220V	Quạt nước tạo dòng FANTO 4 cánh 2HP 220V	d010f78f-b1a6-4244-9590-b6b34703c894	available	2026-05-07 06:46:10.008368+00	2026-05-07 09:43:55.102209+00
95523a4d-f332-4bfb-8e3b-3fb598c8bab3	34	FANTO-SC15-220V	Máy tạo sóng 1 tác dụng FANTO SC-1,5 220V	a8a8c4e1-e3a2-4e11-96ed-b02c9cfd068e	available	2026-05-07 06:46:10.062143+00	2026-05-07 09:43:55.102209+00
9a7563bf-5ebc-4262-a308-845cc28927af	35	FANTO-QN-2C-1HP-380V	Quạt nước tạo dòng FANTO 2 cánh 1HP 380V	d010f78f-b1a6-4244-9590-b6b34703c894	available	2026-05-07 06:46:09.998337+00	2026-05-07 09:43:55.102209+00
9d973d56-cb0e-43f3-be0c-3894b06f629d	36	FANTO-SC075-220V	Máy tạo sóng 1 tác dụng FANTO SC-0,75 220V	a8a8c4e1-e3a2-4e11-96ed-b02c9cfd068e	available	2026-05-07 06:46:10.053751+00	2026-05-07 09:43:55.102209+00
a32266ef-95dc-456c-b3b8-592a6bd7fd4f	12	FANTO-4B-150L	Máy trộn cám FANTO 4B\\150L	f6ff294a-eb36-47b3-82e3-acf886b20e0f	available	2026-05-07 05:32:05.97423+00	2026-05-07 09:43:55.102209+00
a6926cc9-e508-469a-983a-f489d23438a4	37	FANTO-4-BN	Máy cho cá ăn 4 bao nhựa FANTO	531a00d4-ad41-42a5-89b6-b3ddf6016c8e	available	2026-05-07 06:46:09.96766+00	2026-05-07 09:43:55.102209+00
b21e2f12-fbf9-4db3-be8b-4b6a58ca6764	38	FANTO-SO-4C-1300W-B	Máy siêu oxy FANTO 4 cánh 1300W (3.500–5.000 m²)	7aa74630-97fa-4783-8827-2f7e68297707	available	2026-05-07 06:46:10.026937+00	2026-05-07 09:43:55.102209+00
b7c72ddd-c71e-4976-8266-40d3149423c3	39	FANTO-4-IN	Máy cho cá ăn 4 bao inox FANTO	531a00d4-ad41-42a5-89b6-b3ddf6016c8e	available	2026-05-07 06:46:09.980015+00	2026-05-07 09:43:55.102209+00
b7e4b5dd-c224-4850-aa29-acd6ac6d9583	10	FANTO-2B-75L	Máy trộn cám FANTO 2B\\75L	f6ff294a-eb36-47b3-82e3-acf886b20e0f	available	2026-05-07 05:27:29.564954+00	2026-05-07 09:43:55.102209+00
b85e5fbd-d9fa-4779-a904-f598679302b0	40	FANTO-SO-10C-2200W-380V	Máy siêu oxy FANTO 10 cánh 2200W 380V	7aa74630-97fa-4783-8827-2f7e68297707	available	2026-05-07 06:46:10.037231+00	2026-05-07 09:43:55.102209+00
b8c5dbc3-cd82-41ad-8849-a6c7ce66cbc4	41	FANTO-SC075-380V	Máy tạo sóng 1 tác dụng FANTO SC-0,75 380V	a8a8c4e1-e3a2-4e11-96ed-b02c9cfd068e	available	2026-05-07 06:46:10.058247+00	2026-05-07 09:43:55.102209+00
bd6721df-0e0a-48db-92ec-7c43c5d07a8f	11	FANTO-3B-100L	Máy trộn cám FANTO 3B\\100L	f6ff294a-eb36-47b3-82e3-acf886b20e0f	available	2026-05-07 05:29:38.523661+00	2026-05-07 09:43:55.102209+00
bdaf9356-92cb-4a4e-bf01-acf90d233946	42	FANTO-SO-4C-1300W-A	Máy siêu oxy FANTO 4 cánh 1300W (2.500–3.500 m²)	7aa74630-97fa-4783-8827-2f7e68297707	available	2026-05-07 06:46:10.022646+00	2026-05-07 09:43:55.102209+00
cb6c58a1-261c-4b7c-85a2-14e7af4ed9ee	43	FANTO-QN-6C-3HP-220V	Quạt nước tạo dòng FANTO 6 cánh 3HP 220V	d010f78f-b1a6-4244-9590-b6b34703c894	available	2026-05-07 06:46:10.016245+00	2026-05-07 09:43:55.102209+00
d9557595-cb71-4e95-bf23-74c95aa06de1	44	FANTO-SO-6C-1500W-380V	Máy siêu oxy FANTO 6 cánh 1500W 380V	7aa74630-97fa-4783-8827-2f7e68297707	available	2026-05-07 06:46:10.030126+00	2026-05-07 09:43:55.102209+00
dd8cea67-2423-45b4-9f6e-8c751c81cd50	45	FANTO-6-IN-360	Máy cho cá ăn 6 bao inox xoay 360° FANTO	531a00d4-ad41-42a5-89b6-b3ddf6016c8e	available	2026-05-07 06:46:09.983578+00	2026-05-07 09:43:55.102209+00
e5d1e900-9660-49f9-b970-e6b027390dec	46	FANTO-SO-6C-1500W-220V	Máy siêu oxy FANTO 6 cánh 1500W 220V	7aa74630-97fa-4783-8827-2f7e68297707	available	2026-05-07 06:46:10.033495+00	2026-05-07 09:43:55.102209+00
e7297c50-b3b0-418a-8f8a-350030b31cdf	48	FANTO-HRE50L	Máy thổi khí dưới nước FANTO HRE50L	a8e339cc-2954-42c6-ba32-5a2353383dfe	available	2026-05-07 06:46:10.080458+00	2026-05-07 09:43:55.102209+00
\.


--
-- Data for Name: site_settings; Type: TABLE DATA; Schema: public; Owner: aquacms
--

COPY public.site_settings (id, company_name, logo, address, phone, email, facebook, show_facebook, zalo, show_zalo, youtube, show_youtube, tiktok, show_tiktok, telegram, show_telegram, show_hotline, bank_name, bank_number, bank_owner, background_color, primary_color, navbar_color, footer_color, footer_text, show_footer, show_banners, show_categories, show_featured_products, show_latest_posts, show_partners, featured_products_count, latest_posts_count, show_nav_products, show_nav_knowledge, show_nav_partners, show_nav_cart, about_title, about_content, about_image, default_meta_title, default_meta_description, default_og_image, google_analytics_id, facebook_pixel_id, footer_about_text, copyright_text, email_enabled, smtp_host, smtp_port, smtp_use_ssl, smtp_user, smtp_password, smtp_from_email, smtp_from_name, notification_email, chat_auto_reply_message, updated_at, show_nav_about) FROM stdin;
b57a2a7f-4b8e-4702-9f0e-4273965c8cdc	ád	/uploads/settings/85eddb21b13c4e56a70ab970ffd354f5.png	\N	0353785710	contact@cataloga.com	https://www.facebook.com/tranchiduc.1523/	t	0981026888	t	https://www.youtube.com/@chiductran1728	t	\N	f	\N	f	t	\N	\N	\N	#F9F9F9	#55B3D9	#2563eb	#1F2937	Precision Engineering.	t	t	t	t	t	t	8	6	t	t	t	t	aaaaaaa	<p>ádagsdhjasd</p><p>adjhaksjđ</p><p>adashjdasjkdaaaaaaa</p><p><img src="/uploads/content/29aa16137acb4747b4637c384858c4cd.jpg"></p>	/uploads/settings/beab110c755a43038d0ef5a6deab4b51.png	\N	\N	\N	\N	\N		\N	f	\N	587	t	\N	\N	\N	\N	\N	xin chào	2026-05-14 10:11:10.101401+00	t
\.


--
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: aquacms
--

COPY public.users (id, name, email, password_hash, role, avatar, is_active, last_login_at, created_at, updated_at) FROM stdin;
4a9f31f5-6514-480c-a329-cd41b9fcc473	System Admin	admin@cataloga.com	$argon2id$v=19$m=65536,t=3,p=1$uKfDeHHg40EXcdR4sguNtw$z5k514OYZgnhMQaSCXwucos6k45pRjd+lSQLbZQmPm8	SUPER_ADMIN	\N	t	\N	2026-05-05 02:45:04.280587+00	2026-05-05 02:45:37.734802+00
2c3e30b2-950a-4939-abe5-6a11aa1331d3	System Admin	admin@aquacms.com	$argon2id$v=19$m=65536,t=3,p=1$Uu4rWwH4kSEvnMdN6nAFnA$3/1LrEjr7L22dpNE3hVFXNeXDvOMgGT06/mHsGGwA4c	SUPER_ADMIN	\N	t	2026-05-13 09:33:42.997678+00	2026-05-05 02:45:35.336185+00	2026-05-13 09:33:42.997678+00
\.


--
-- Name: page_views_id_seq; Type: SEQUENCE SET; Schema: public; Owner: aquacms
--

SELECT pg_catalog.setval('public.page_views_id_seq', 198, true);


--
-- Name: partners_short_id_seq; Type: SEQUENCE SET; Schema: public; Owner: aquacms
--

SELECT pg_catalog.setval('public.partners_short_id_seq', 1, true);


--
-- Name: posts_short_id_seq; Type: SEQUENCE SET; Schema: public; Owner: aquacms
--

SELECT pg_catalog.setval('public.posts_short_id_seq', 1, true);


--
-- Name: products_short_id_seq; Type: SEQUENCE SET; Schema: public; Owner: aquacms
--

SELECT pg_catalog.setval('public.products_short_id_seq', 51, true);


--
-- Name: activity_logs activity_logs_pkey; Type: CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.activity_logs
    ADD CONSTRAINT activity_logs_pkey PRIMARY KEY (id);


--
-- Name: banners banners_pkey; Type: CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.banners
    ADD CONSTRAINT banners_pkey PRIMARY KEY (id);


--
-- Name: categories categories_pkey; Type: CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.categories
    ADD CONSTRAINT categories_pkey PRIMARY KEY (id);


--
-- Name: categories categories_slug_key; Type: CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.categories
    ADD CONSTRAINT categories_slug_key UNIQUE (slug);


--
-- Name: chat_messages chat_messages_pkey; Type: CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.chat_messages
    ADD CONSTRAINT chat_messages_pkey PRIMARY KEY (id);


--
-- Name: chat_sessions chat_sessions_guest_id_key; Type: CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.chat_sessions
    ADD CONSTRAINT chat_sessions_guest_id_key UNIQUE (guest_id);


--
-- Name: chat_sessions chat_sessions_pkey; Type: CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.chat_sessions
    ADD CONSTRAINT chat_sessions_pkey PRIMARY KEY (id);


--
-- Name: knowledge_categories knowledge_categories_pkey; Type: CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.knowledge_categories
    ADD CONSTRAINT knowledge_categories_pkey PRIMARY KEY (id);


--
-- Name: knowledge_categories knowledge_categories_slug_key; Type: CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.knowledge_categories
    ADD CONSTRAINT knowledge_categories_slug_key UNIQUE (slug);


--
-- Name: page_views page_views_pkey; Type: CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.page_views
    ADD CONSTRAINT page_views_pkey PRIMARY KEY (id);


--
-- Name: partner_categories partner_categories_pkey; Type: CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.partner_categories
    ADD CONSTRAINT partner_categories_pkey PRIMARY KEY (id);


--
-- Name: partner_categories partner_categories_slug_key; Type: CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.partner_categories
    ADD CONSTRAINT partner_categories_slug_key UNIQUE (slug);


--
-- Name: partners partners_pkey; Type: CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.partners
    ADD CONSTRAINT partners_pkey PRIMARY KEY (id);


--
-- Name: partners partners_short_id_key; Type: CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.partners
    ADD CONSTRAINT partners_short_id_key UNIQUE (short_id);


--
-- Name: partners partners_slug_key; Type: CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.partners
    ADD CONSTRAINT partners_slug_key UNIQUE (slug);


--
-- Name: posts posts_pkey; Type: CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.posts
    ADD CONSTRAINT posts_pkey PRIMARY KEY (id);


--
-- Name: posts posts_short_id_key; Type: CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.posts
    ADD CONSTRAINT posts_short_id_key UNIQUE (short_id);


--
-- Name: posts posts_slug_key; Type: CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.posts
    ADD CONSTRAINT posts_slug_key UNIQUE (slug);


--
-- Name: product_contents product_contents_pkey; Type: CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.product_contents
    ADD CONSTRAINT product_contents_pkey PRIMARY KEY (product_id);


--
-- Name: product_finances product_finances_pkey; Type: CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.product_finances
    ADD CONSTRAINT product_finances_pkey PRIMARY KEY (product_id);


--
-- Name: product_metadata product_metadata_pkey; Type: CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.product_metadata
    ADD CONSTRAINT product_metadata_pkey PRIMARY KEY (product_id);


--
-- Name: product_metadata product_metadata_slug_key; Type: CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.product_metadata
    ADD CONSTRAINT product_metadata_slug_key UNIQUE (slug);


--
-- Name: product_statistics product_statistics_pkey; Type: CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.product_statistics
    ADD CONSTRAINT product_statistics_pkey PRIMARY KEY (product_id);


--
-- Name: products products_pkey; Type: CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.products
    ADD CONSTRAINT products_pkey PRIMARY KEY (id);


--
-- Name: products products_short_id_key; Type: CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.products
    ADD CONSTRAINT products_short_id_key UNIQUE (short_id);


--
-- Name: products products_sku_key; Type: CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.products
    ADD CONSTRAINT products_sku_key UNIQUE (sku);


--
-- Name: site_settings site_settings_pkey; Type: CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.site_settings
    ADD CONSTRAINT site_settings_pkey PRIMARY KEY (id);


--
-- Name: users users_email_key; Type: CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_email_key UNIQUE (email);


--
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);


--
-- Name: idx_activity_logs_created; Type: INDEX; Schema: public; Owner: aquacms
--

CREATE INDEX idx_activity_logs_created ON public.activity_logs USING btree (created_at DESC);


--
-- Name: idx_activity_logs_severity; Type: INDEX; Schema: public; Owner: aquacms
--

CREATE INDEX idx_activity_logs_severity ON public.activity_logs USING btree (severity, created_at DESC);


--
-- Name: idx_categories_slug; Type: INDEX; Schema: public; Owner: aquacms
--

CREATE INDEX idx_categories_slug ON public.categories USING btree (slug);


--
-- Name: idx_page_views_path; Type: INDEX; Schema: public; Owner: aquacms
--

CREATE INDEX idx_page_views_path ON public.page_views USING btree (path, viewed_at DESC);


--
-- Name: idx_page_views_viewed_at; Type: INDEX; Schema: public; Owner: aquacms
--

CREATE INDEX idx_page_views_viewed_at ON public.page_views USING btree (viewed_at DESC);


--
-- Name: idx_partners_slug; Type: INDEX; Schema: public; Owner: aquacms
--

CREATE INDEX idx_partners_slug ON public.partners USING btree (slug);


--
-- Name: idx_posts_category; Type: INDEX; Schema: public; Owner: aquacms
--

CREATE INDEX idx_posts_category ON public.posts USING btree (knowledge_category_id);


--
-- Name: idx_posts_slug; Type: INDEX; Schema: public; Owner: aquacms
--

CREATE INDEX idx_posts_slug ON public.posts USING btree (slug);


--
-- Name: idx_product_finances_featured; Type: INDEX; Schema: public; Owner: aquacms
--

CREATE INDEX idx_product_finances_featured ON public.product_finances USING btree (is_featured) WHERE (is_featured = true);


--
-- Name: idx_product_metadata_slug; Type: INDEX; Schema: public; Owner: aquacms
--

CREATE INDEX idx_product_metadata_slug ON public.product_metadata USING btree (slug);


--
-- Name: idx_products_category; Type: INDEX; Schema: public; Owner: aquacms
--

CREATE INDEX idx_products_category ON public.products USING btree (category_id);


--
-- Name: idx_products_name_trgm; Type: INDEX; Schema: public; Owner: aquacms
--

CREATE INDEX idx_products_name_trgm ON public.products USING gin (name public.gin_trgm_ops);


--
-- Name: idx_products_sku; Type: INDEX; Schema: public; Owner: aquacms
--

CREATE INDEX idx_products_sku ON public.products USING btree (sku);


--
-- Name: idx_products_status; Type: INDEX; Schema: public; Owner: aquacms
--

CREATE INDEX idx_products_status ON public.products USING btree (status);


--
-- Name: idx_users_email; Type: INDEX; Schema: public; Owner: aquacms
--

CREATE INDEX idx_users_email ON public.users USING btree (email);


--
-- Name: chat_sessions trg_chat_sessions_updated; Type: TRIGGER; Schema: public; Owner: aquacms
--

CREATE TRIGGER trg_chat_sessions_updated BEFORE UPDATE ON public.chat_sessions FOR EACH ROW EXECUTE FUNCTION public.fn_update_timestamp();


--
-- Name: partners trg_partners_updated; Type: TRIGGER; Schema: public; Owner: aquacms
--

CREATE TRIGGER trg_partners_updated BEFORE UPDATE ON public.partners FOR EACH ROW EXECUTE FUNCTION public.fn_update_timestamp();


--
-- Name: posts trg_posts_updated; Type: TRIGGER; Schema: public; Owner: aquacms
--

CREATE TRIGGER trg_posts_updated BEFORE UPDATE ON public.posts FOR EACH ROW EXECUTE FUNCTION public.fn_update_timestamp();


--
-- Name: products trg_products_updated; Type: TRIGGER; Schema: public; Owner: aquacms
--

CREATE TRIGGER trg_products_updated BEFORE UPDATE ON public.products FOR EACH ROW EXECUTE FUNCTION public.fn_update_timestamp();


--
-- Name: site_settings trg_settings_updated; Type: TRIGGER; Schema: public; Owner: aquacms
--

CREATE TRIGGER trg_settings_updated BEFORE UPDATE ON public.site_settings FOR EACH ROW EXECUTE FUNCTION public.fn_update_timestamp();


--
-- Name: users trg_users_updated; Type: TRIGGER; Schema: public; Owner: aquacms
--

CREATE TRIGGER trg_users_updated BEFORE UPDATE ON public.users FOR EACH ROW EXECUTE FUNCTION public.fn_update_timestamp();


--
-- Name: activity_logs activity_logs_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.activity_logs
    ADD CONSTRAINT activity_logs_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE SET NULL;


--
-- Name: chat_messages chat_messages_session_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.chat_messages
    ADD CONSTRAINT chat_messages_session_id_fkey FOREIGN KEY (session_id) REFERENCES public.chat_sessions(id) ON DELETE CASCADE;


--
-- Name: partners partners_partner_category_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.partners
    ADD CONSTRAINT partners_partner_category_id_fkey FOREIGN KEY (partner_category_id) REFERENCES public.partner_categories(id) ON DELETE SET NULL;


--
-- Name: posts posts_knowledge_category_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.posts
    ADD CONSTRAINT posts_knowledge_category_id_fkey FOREIGN KEY (knowledge_category_id) REFERENCES public.knowledge_categories(id) ON DELETE SET NULL;


--
-- Name: product_contents product_contents_product_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.product_contents
    ADD CONSTRAINT product_contents_product_id_fkey FOREIGN KEY (product_id) REFERENCES public.products(id) ON DELETE CASCADE;


--
-- Name: product_finances product_finances_product_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.product_finances
    ADD CONSTRAINT product_finances_product_id_fkey FOREIGN KEY (product_id) REFERENCES public.products(id) ON DELETE CASCADE;


--
-- Name: product_metadata product_metadata_product_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.product_metadata
    ADD CONSTRAINT product_metadata_product_id_fkey FOREIGN KEY (product_id) REFERENCES public.products(id) ON DELETE CASCADE;


--
-- Name: product_statistics product_statistics_product_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.product_statistics
    ADD CONSTRAINT product_statistics_product_id_fkey FOREIGN KEY (product_id) REFERENCES public.products(id) ON DELETE CASCADE;


--
-- Name: products products_category_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: aquacms
--

ALTER TABLE ONLY public.products
    ADD CONSTRAINT products_category_id_fkey FOREIGN KEY (category_id) REFERENCES public.categories(id) ON DELETE SET NULL;


--
-- PostgreSQL database dump complete
--

\unrestrict ICyHUNnxm4ZwxKxkGz0AItkN4PpiNrEISeynLiNB8g4f6skXWOHDCGCY8YBcPuo

